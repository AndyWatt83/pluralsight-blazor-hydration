using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BlazorBloggingApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorBloggingApp.Tests.LearningOutcomes.Module4;

/// <summary>
/// Module 4 — Shared State Without the Singleton Trap (TO6, TO5).
/// Clip 1 shows the AddSingleton leak live in two browsers; these tests pin the
/// same claims: a server singleton is shared by every user, a scoped AppState
/// needs a real home (the theme cookie), and RegisterPersistentService hands the
/// SSR load's AppState to the WebAssembly runtime.
/// </summary>
public class SharedStateTests : IClassFixture<BlogAppFixture>
{
    private readonly BlogAppFixture _app;

    public SharedStateTests(BlogAppFixture app) => _app = app;

    // ── M4C1: The trap ──────────────────────────────────────────────────────

    [Fact]
    [Lo("TO6", "M4C1")]
    public void SingletonAppState_IsSharedAcrossUsers_TheLeak()
    {
        // A singleton is one instance for the whole server process. Two DI
        // scopes stand in for two users — user B sees user A's change.
        var services = new ServiceCollection();
        services.AddSingleton<AppState>();
        using var provider = services.BuildServiceProvider();

        using var userA = provider.CreateScope();
        using var userB = provider.CreateScope();
        userA.ServiceProvider.GetRequiredService<AppState>().Theme = "dark";

        Assert.Equal("dark", userB.ServiceProvider.GetRequiredService<AppState>().Theme);
    }

    [Fact]
    [Lo("TO6", "M4C1")]
    public void TheApp_RegistersAppStateScoped_NotSingleton()
    {
        // The same check against the real app's container: two scopes, two
        // instances. Flipping the registration back to AddSingleton fails this.
        using var scopeA = _app.Services.CreateScope();
        using var scopeB = _app.Services.CreateScope();

        Assert.NotSame(
            scopeA.ServiceProvider.GetRequiredService<AppState>(),
            scopeB.ServiceProvider.GetRequiredService<AppState>());
    }

    // ── M4C2: Where the theme lives ─────────────────────────────────────────

    [Fact]
    [Lo("TO6", "M4C2")]
    public async Task ThemeToggle_ChangesOnlyThatUsersTheme()
    {
        // Two clients, two cookie jars — the two browsers from the demo.
        using var userA = _app.CreateClient();
        using var userB = _app.CreateClient();

        await ToggleThemeAsync(userA);

        Assert.True(IsDarkPage(await userA.GetStringAsync("/")));
        Assert.False(IsDarkPage(await userB.GetStringAsync("/")));
    }

    [Fact]
    [Lo("TO6", "M4C2")]
    public async Task ThemeCookie_IsCopiedIntoAppState_DuringTheSsrLoad()
    {
        // A fresh scoped AppState is made for every request; the cookie is what
        // the SSR load copies the user's theme from.
        var html = await GetWithThemeCookieAsync($"/post/{BlogAppFixture.IntroPostId}", "dark");

        Assert.True(IsDarkPage(html));
        Assert.Contains("post-article card shadow-sm text-bg-dark", html);
    }

    // ── M4C3: Handing AppState to the interactivity ─────────────────────────

    [Fact]
    [Lo("TO5", "M4C3")]
    public void AppStateTheme_IsDeclaredPersistent()
    {
        var attr = typeof(AppState).GetProperty(nameof(AppState.Theme))!
            .GetCustomAttribute<PersistentStateAttribute>();

        Assert.NotNull(attr);
    }

    [Fact]
    [Lo("TO5", "M4C3")]
    public async Task PersistedState_NamesAppState_AndCarriesTheTheme()
    {
        // RegisterPersistentService writes AppState's [PersistentState] values
        // into the WebAssembly state comment, next to a registry entry that
        // names the service type in plain text.
        var html = await GetWithThemeCookieAsync($"/post/{BlogAppFixture.IntroPostId}", "dark");
        var marker = Regex.Match(html, "<!--Blazor-WebAssembly-Component-State:([A-Za-z0-9+/=]+)-->");
        Assert.True(marker.Success);

        var entries = JsonSerializer.Deserialize<Dictionary<string, string>>(
            Encoding.UTF8.GetString(Convert.FromBase64String(marker.Groups[1].Value)))!;
        var values = entries.Values.Select(Decode).ToList();

        var registry = entries.Single(e => e.Key.EndsWith("PersistentServicesRegistry")).Value;
        Assert.Contains("BlazorBloggingApp.Services.AppState", Decode(registry));
        Assert.Contains("\"dark\"", values);
    }

    // ── helpers ─────────────────────────────────────────────────────────────

    private static bool IsDarkPage(string html) => html.Contains("class=\"page theme-dark\"");

    private static string Decode(string base64)
    {
        try { return Encoding.UTF8.GetString(Convert.FromBase64String(base64)); }
        catch (FormatException) { return base64; }
    }

    private async Task<string> GetWithThemeCookieAsync(string path, string theme)
    {
        using var client = _app.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Add("Cookie", $"theme={theme}");
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private static async Task ToggleThemeAsync(HttpClient client)
    {
        // Submit the ThemeToggle form exactly as the browser does: the
        // antiforgery token and the hidden fields come from the rendered page.
        var page = await client.GetStringAsync("/");
        string Field(string name) =>
            Regex.Match(page, $"name=\"{name}\"[^>]*value=\"([^\"]*)\"").Groups[1].Value;

        var response = await client.PostAsync("/preferences/theme", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["theme"] = Field("theme"),
            ["returnUrl"] = Field("returnUrl"),
            ["__RequestVerificationToken"] = Field("__RequestVerificationToken"),
        }));
        response.EnsureSuccessStatusCode();
    }
}
