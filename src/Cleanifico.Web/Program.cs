using Cleanifico.Web.Components;
using Cleanifico.Web.ApiClients;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var cleanificoApiBaseUrl = builder.Configuration["CleanificoApi:BaseUrl"];

if (!Uri.TryCreate(cleanificoApiBaseUrl, UriKind.Absolute, out var cleanificoApiUri))
{
    throw new InvalidOperationException(
        "The setting 'CleanificoApi:BaseUrl' must contain an absolute URI.");
}

builder.Services.AddHttpClient<ICleaningTypesApiClient, CleaningTypesApiClient>(client =>
    client.BaseAddress = cleanificoApiUri);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
