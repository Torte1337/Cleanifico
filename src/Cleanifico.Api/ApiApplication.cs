using Cleanifico.Api.Endpoints;
using Cleanifico.Api.ErrorHandling;
using Cleanifico.Application.CleaningTypes;
using Cleanifico.Infrastructure;

namespace Cleanifico.Api;

public static class ApiApplication
{
    public static WebApplication Build(
        string[] args,
        Action<IServiceCollection>? configureServices = null)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            ApplicationName = typeof(ApiApplication).Assembly.FullName
        });

        builder.Services.AddExceptionHandler<ApiExceptionHandler>();
        builder.Services.AddProblemDetails();
        builder.Services.AddHealthChecks();
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddScoped<ICleaningTypeService, CleaningTypeService>();

        var connectionString = builder.Configuration.GetConnectionString(
            DependencyInjection.ConnectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "The connection string 'ConnectionStrings:Cleanifico' is not configured.");
        }

        builder.Services.AddCleanificoInfrastructure(connectionString);
        configureServices?.Invoke(builder.Services);

        var app = builder.Build();

        app.UseExceptionHandler();

        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.UseHttpsRedirection();
        }

        app.MapHealthChecks("/health");
        app.MapCleaningTypeEndpoints();

        return app;
    }
}
