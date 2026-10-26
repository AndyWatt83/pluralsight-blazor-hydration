using BlazorBloggingApp.Components;
using BlazorBloggingApp.Models;
using BlazorBloggingApp.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// The in-memory client doubles as the app's data store, so it stays a singleton
// on the server. WebAssembly clients get the HTTP implementation instead — see
// the Client project's Program.cs.
builder.Services.AddSingleton<IBlogApiClient, InMemoryBlogApiClient>();

// Diagnostic only: lets InMemoryBlogApiClient log which render path called it.
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, DemoAuthStateProvider>();

// One AppState per user.
builder.Services.AddScoped<AppState>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

// The blog API — what the WebAssembly runtime calls after the Auto handoff.
app.MapGet("/api/posts", async (IBlogApiClient api)
    => await api.GetAllPostsAsync());
app.MapGet("/api/posts/{id:guid}", async (Guid id, IBlogApiClient api)
    => await api.GetPostByIdAsync(id) is BlogPost post
        ? Results.Ok(post)
        : Results.NotFound());
app.MapPut("/api/posts/{id:guid}", async (Guid id, UpdatePostRequest update, IBlogApiClient api) =>
{
    await api.UpdatePostAsync(id, update.Title, update.Content);
    return Results.NoContent();
});

// The theme lives in a cookie, so it arrives with every request...
app.MapPost("/preferences/theme", (HttpContext context, [FromForm] string theme, [FromForm] string returnUrl) =>
{
    context.Response.Cookies.Append("theme", theme, new CookieOptions
    {
        HttpOnly = true,
        SameSite = SameSiteMode.Lax,
        MaxAge = TimeSpan.FromDays(365)
    });
    return Results.LocalRedirect(returnUrl);
});

// ...and each request copies it into that user's AppState before rendering.
app.Use(async (context, next) =>
{
    var appState = context.RequestServices.GetRequiredService<AppState>();
    appState.Theme = context.Request.Cookies["theme"] ?? "light";
    await next();
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorBloggingApp.Client._Imports).Assembly);

app.Run();

internal record UpdatePostRequest(string Title, string Content);

// Exposes the entry point to WebApplicationFactory for the integration tests
// that assert on real prerendered HTML (the learning-outcome suite).
public partial class Program { }
