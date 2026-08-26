using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cleanifico.Application.Licensing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Cleanifico.Infrastructure.Licensing;

public sealed class FileLocalLicenseStore : ILocalLicenseStore, IDisposable
{
    private const int MaximumStateBytes = 2 * 1024 * 1024;
    private static readonly UnixFileMode PrivateFileMode =
        UnixFileMode.UserRead | UnixFileMode.UserWrite;
    private static readonly UnixFileMode ForbiddenFileMode =
        UnixFileMode.GroupRead | UnixFileMode.GroupWrite | UnixFileMode.GroupExecute
        | UnixFileMode.OtherRead | UnixFileMode.OtherWrite | UnixFileMode.OtherExecute;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly string statePath;

    public FileLocalLicenseStore(
        IOptions<LicensingOptions> options,
        IHostEnvironment hostEnvironment)
    {
        string configuredPath = string.IsNullOrWhiteSpace(options.Value.StatePath)
            ? LicensingOptions.DefaultStatePath
            : options.Value.StatePath;
        if (configuredPath.Contains('\0'))
        {
            throw new ArgumentException("Licensing:StatePath ist ungültig.", nameof(options));
        }

        statePath = Path.GetFullPath(configuredPath, hostEnvironment.ContentRootPath);
        if (string.IsNullOrWhiteSpace(Path.GetFileName(statePath)))
        {
            throw new ArgumentException("Licensing:StatePath muss auf eine Datei zeigen.", nameof(options));
        }
    }

    public async Task<LocalLicenseLoadResult> LoadAsync(
        CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            return await LoadCoreAsync(cancellationToken);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task SaveAsync(
        LocalLicenseState state,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (state.SignedLicenseLease is { Payload.InstallationId: var leaseInstallationId }
            && leaseInstallationId != state.InstallationId)
        {
            throw new ArgumentException("Der lokale License State ist ungültig.", nameof(state));
        }

        await gate.WaitAsync(cancellationToken);
        try
        {
            await SaveCoreAsync(state, cancellationToken);
        }
        finally
        {
            gate.Release();
        }
    }

    public void Dispose() => gate.Dispose();

    private async Task<LocalLicenseLoadResult> LoadCoreAsync(CancellationToken cancellationToken)
    {
        try
        {
            EnsurePathHasNoSymbolicLinks(statePath);
            if (!File.Exists(statePath))
            {
                return new(LocalLicenseLoadStatus.NotFound, null);
            }

            var info = new FileInfo(statePath);
            if (info.LinkTarget is not null
                || info.Length is <= 0 or > MaximumStateBytes
                || !OperatingSystem.IsWindows()
                && (File.GetUnixFileMode(statePath) & ForbiddenFileMode) != 0)
            {
                return new(LocalLicenseLoadStatus.Invalid, null);
            }

            await using var stream = new FileStream(
                statePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4096,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            LocalLicenseState? state = await JsonSerializer.DeserializeAsync<LocalLicenseState>(
                stream,
                JsonOptions,
                cancellationToken);
            if (state is null
                || state.SignedLicenseLease is { Payload.InstallationId: var leaseInstallationId }
                && leaseInstallationId != state.InstallationId)
            {
                return new(LocalLicenseLoadStatus.Invalid, null);
            }

            return new(LocalLicenseLoadStatus.Success, state);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (exception is JsonException
                                          or ArgumentException
                                          or NotSupportedException)
        {
            return new(LocalLicenseLoadStatus.Invalid, null);
        }
        catch (Exception exception) when (exception is IOException
                                          or UnauthorizedAccessException)
        {
            return new(LocalLicenseLoadStatus.Unavailable, null);
        }
    }

    private async Task SaveCoreAsync(
        LocalLicenseState state,
        CancellationToken cancellationToken)
    {
        LocalLicenseLoadResult existing = await LoadCoreAsync(cancellationToken);
        if (existing.Succeeded && existing.State!.InstallationId != state.InstallationId)
        {
            throw new InvalidOperationException("Eine bestehende InstallationId darf nicht ersetzt werden.");
        }

        if (existing.Status == LocalLicenseLoadStatus.Invalid)
        {
            throw new InvalidDataException("Ein beschädigter License State wird nicht überschrieben.");
        }

        if (existing.Status == LocalLicenseLoadStatus.Unavailable)
        {
            throw new IOException("Der lokale License State ist nicht sicher verfügbar.");
        }

        string directory = Path.GetDirectoryName(statePath)
            ?? throw new InvalidOperationException("Licensing:StatePath benötigt ein Verzeichnis.");
        EnsurePathHasNoSymbolicLinks(directory);
        Directory.CreateDirectory(directory);
        EnsurePathHasNoSymbolicLinks(directory);

        string temporaryPath = Path.Combine(
            directory,
            $".{Path.GetFileName(statePath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            byte[] bytes = Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(state, JsonOptions) + "\n");
            if (bytes.Length > MaximumStateBytes)
            {
                throw new InvalidOperationException("Der lokale License State ist zu groß.");
            }

            var streamOptions = new FileStreamOptions
            {
                Mode = FileMode.CreateNew,
                Access = FileAccess.Write,
                Share = FileShare.None,
                BufferSize = 4096,
                Options = FileOptions.Asynchronous | FileOptions.WriteThrough
            };
            if (!OperatingSystem.IsWindows())
            {
                streamOptions.UnixCreateMode = PrivateFileMode;
            }

            await using (var stream = new FileStream(temporaryPath, streamOptions))
            {
                await stream.WriteAsync(bytes, cancellationToken);
                await stream.FlushAsync(cancellationToken);
                stream.Flush(flushToDisk: true);
            }

            EnsurePathHasNoSymbolicLinks(statePath);
            File.Move(temporaryPath, statePath, overwrite: true);
            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(statePath, PrivateFileMode);
            }
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private static void EnsurePathHasNoSymbolicLinks(string path)
    {
        string fullPath = Path.GetFullPath(path);
        string root = Path.GetPathRoot(fullPath)
            ?? throw new ArgumentException("Der License-State-Pfad besitzt kein Stammverzeichnis.", nameof(path));
        string current = root;
        foreach (string segment in fullPath[root.Length..]
                     .Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
        {
            current = Path.Combine(current, segment);
            FileSystemInfo info = Directory.Exists(current)
                ? new DirectoryInfo(current)
                : new FileInfo(current);
            if (info.LinkTarget is not null)
            {
                throw new IOException("Der License-State-Pfad enthält einen symbolischen Link.");
            }

            if (!Directory.Exists(current) && !File.Exists(current))
            {
                break;
            }
        }
    }
}
