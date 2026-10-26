using BlazorBloggingApp.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// On WebAssembly the blog data comes over HTTP from the server's API endpoints —
// the same IBlogApiClient abstraction the server satisfies in memory.
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IBlogApiClient, BlogApiClient>();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, DemoAuthStateProvider>();

// In the browser there's only one user, so one AppState is right.
builder.Services.AddSingleton<AppState>();

await builder.Build().RunAsync();
