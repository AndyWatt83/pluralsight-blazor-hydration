using System;
using System.Threading.Tasks;
using BlazorBloggingApp.Client.Pages;
using BlazorBloggingApp.Models;
using BlazorBloggingApp.Services;
using Microsoft.AspNetCore.Components;
using NSubstitute;

namespace BlazorBloggingApp.Tests.LearningOutcomes.Module2;

/// <summary>
/// Module 2 — Persisted State (TO2, TO3).
/// The Module 1 characterization tests, flipped now that the bug is fixed:
/// [PersistentState] writes the post into the page as a state comment, and
/// PostDetail doesn't fetch again when it already has the post it was asked for.
/// </summary>
public class PersistedStateTests : BunitContext, IClassFixture<BlogAppFixture>
{
    private readonly BlogAppFixture _app;
    private readonly IBlogApiClient _mockApi = Substitute.For<IBlogApiClient>();

    public PersistedStateTests(BlogAppFixture app)
    {
        _app = app;
        Services.AddSingleton(_mockApi);
        Services.AddSingleton(new AppState()); // PostDetail reads the theme (Module 4)
        this.AddAuthorization();
        SetRendererInfo(new RendererInfo("Static", isInteractive: false));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    [Lo("TO3", "M2C1")]
    public async Task PostDetail_WithThePostAlreadyLoaded_DoesNotFetchAgain()
    {
        // Module 1 pinned two fetches here. Once the post is held in persisted
        // state, another lifecycle run for the same post reuses it.
        var postId = Guid.NewGuid();
        _mockApi.GetPostByIdAsync(postId).Returns(new BlogPost
        {
            Id = postId, Title = "Any", Content = "Any"
        });

        var cut = Render<PostDetail>(p => p.Add(pd => pd.PostId, postId));
        cut.WaitForElement(".post-article");
        await _mockApi.Received(1).GetPostByIdAsync(postId);

        cut.Render(p => p.Add(pd => pd.PostId, postId));
        cut.WaitForElement(".post-article");
        await _mockApi.Received(1).GetPostByIdAsync(postId);
    }

    [Fact]
    [Lo("TO2", "M2C2")]
    public async Task PrerenderedHtml_CarriesThePersistedStatePayload()
    {
        // Module 1 pinned the absence of this payload. PostDetail runs in
        // WebAssembly, so the post travels in the WebAssembly state comment.
        var html = await _app.GetPrerenderedHtmlAsync($"/post/{BlogAppFixture.IntroPostId}");

        Assert.Contains("Blazor-WebAssembly-Component-State", html);
    }
}
