using System.Xml.Linq;

namespace Cleanifico.Tests.Architecture;

internal static class RepositoryStructure
{
    public static IReadOnlyList<string> ReadProjectReferences(string projectRelativePath)
    {
        var document = XDocument.Load(Path.Combine(FindRepositoryRoot(), projectRelativePath));

        return document
            .Descendants("ProjectReference")
            .Select(element => element.Attribute("Include")?.Value)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => GetProjectName(path!))
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    public static IReadOnlyList<string> ReadSolutionProjects()
    {
        var document = XDocument.Load(Path.Combine(FindRepositoryRoot(), "Cleanifico.slnx"));

        return document
            .Descendants("Project")
            .Select(element => element.Attribute("Path")?.Value)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => GetProjectName(path!))
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Cleanifico.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("The Cleanifico repository root could not be found.");
    }

    private static string GetProjectName(string path)
    {
        var normalizedPath = path.Replace('\\', Path.DirectorySeparatorChar);
        return Path.GetFileNameWithoutExtension(normalizedPath);
    }
}
