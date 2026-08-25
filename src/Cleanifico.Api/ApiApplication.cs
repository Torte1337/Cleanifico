namespace Cleanifico.Api;

public static class ApiApplication
{
    public static WebApplication Build(string[] args)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            ApplicationName = typeof(ApiApplication).Assembly.FullName
        });

        builder.Services.AddProblemDetails();
        builder.Services.AddHealthChecks();

        var app = builder.Build();

        app.UseExceptionHandler();

        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.UseHttpsRedirection();
        }

        app.MapHealthChecks("/health");

        return app;
    }
}
