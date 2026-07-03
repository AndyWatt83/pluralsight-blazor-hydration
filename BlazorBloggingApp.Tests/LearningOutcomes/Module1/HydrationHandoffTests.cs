using System;
using System.Threading.Tasks;
using BlazorBloggingApp.Client.Pages;
using BlazorBloggingApp.Models;
using BlazorBloggingApp.Services;
using Microsoft.AspNetCore.Components;
using NSubstitute;

namespace BlazorBloggingApp.Tests.LearningOutcomes.Module1;

/// <summary>
/// Module 1 — The Hydration Handoff (TO1).
/// Pins the problem the course opens with: the server renders the post during
/// prerendering, and the interactive runtime then throws that work away and
/// fetches again. These are characterization tests — they document the bug on
/// purpose, and Module 2 rewrites them when the bug is fixed.
/// </summary>
public class HydrationHandoffTests : BunitContext, IClassFixture<BlogAppFixture>
{
    private readonly BlogAppFixture _app;
    private readonly IBlogApiClient _mockApi = Substitute.For<IBlogApiClient>();

    public HydrationHandoffTests(BlogAppFixture app)
    {
        _app = app;
        Services.AddSingleton(_mockApi);
        this.AddAuthorization();
        SetRendererInfo(new RendererInfo("Static", isInteractive: false));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    [Lo("TO1", "M1C2")]
    public async Task PostDetail_FetchesOnEveryLifecycleRun_NothingCarriesOver()
    {
        // The prerender pass and the interactive pass are separate lifecycle
        // runs. PostDetail's naive fetch has no memory between them — so a hard
        // reload costs two fetches and a visible flicker. bUnit shows the
        // component-level half: every lifecycle run pays for its own fetch.
        var postId = Guid.NewGuid();
        _mockApi.GetPostByIdAsync(postId).Returns(new BlogPost
        {
            Id = postId, Title = "Any", Content = "Any"
        });

        var cut = Render<PostDetail>(p => p.Add(pd => pd.PostId, postId));
        cut.WaitForElement(".post-article");
        await _mockApi.Received(1).GetPostByIdAsync(postId);

        // A second lifecycle run (what the interactive boot effectively is)
        // fetches again — nothing remembered the prerendered data.
        cut.Render(p => p.Add(pd => pd.PostId, postId));
        cut.WaitForElement(".post-article");
        await _mockApi.Received(2).GetPostByIdAsync(postId);
    }

    [Fact]
    [Lo("TO1", "M1C3")]
    public async Task PrerenderedHtml_AlreadyContainsThePostContent()
    {
        // "The data was already there" — the server fetched the post and
        // rendered it into the HTML that reaches the browser.
        var html = await _app.GetPrerenderedHtmlAsync($"/post/{BlogAppFixture.IntroPostId}");

        Assert.Contains("Introduction to Blazor", html);
        Assert.Contains("data-testid=\"post-content\"", html);
    }

    [Fact]
    [Lo("TO1", "M1C3")]
    public async Task NoPersistedStatePayload_ExistsBeforeTheFix()
    {
        // Module 1's app has no [PersistentState] — so nothing carries the
        // prerendered data across the boundary. Module 2 flips this assertion.
        var html = await _app.GetPrerenderedHtmlAsync($"/post/{BlogAppFixture.IntroPostId}");

        Assert.DoesNotContain("blazor-component-state", html, StringComparison.OrdinalIgnoreCase);
    }
}
