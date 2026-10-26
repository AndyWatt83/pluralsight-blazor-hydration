using BlazorBloggingApp.Client.Pages;
using BlazorBloggingApp.Models;
using BlazorBloggingApp.Services;
using Microsoft.AspNetCore.Components;
using NSubstitute;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorBloggingApp.Tests;

public class PostDetailTests : BunitContext
{
    private readonly IBlogApiClient _mockApi = Substitute.For<IBlogApiClient>();
    private readonly BunitAuthorizationContext _authContext;

    public PostDetailTests()
    {
        Services.AddSingleton(_mockApi);
        Services.AddSingleton(new AppState()); // PostDetail reads the theme (Module 4)
        _authContext = this.AddAuthorization();
        SetRendererInfo(new RendererInfo("Static", isInteractive: false));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void DisplaysPostTitleAndContent()
    {
        var postId = Guid.NewGuid();
        _mockApi.GetPostByIdAsync(postId).Returns(new BlogPost
        {
            Id = postId,
            Title = "Testing with bUnit",
            Content = "The workhorse tool for testing Blazor apps is...",
            PublishedDate = new DateTime(2026, 3, 20),
            HeaderImageUrl = "https://example.com/test.jpg"
        });

        var cut = Render<PostDetail>(p => p
            .Add(pd => pd.PostId, postId)
        );

        cut.WaitForAssertion(() =>
        {
            cut.Find(".card-title").MarkupMatches(
                @"<h2 class=""card-title mb-3"" data-testid=""post-title"">Testing with bUnit</h2>");

            cut.Find(".card-text").MarkupMatches(
                @"<div class=""card-text"" data-testid=""post-content"">The workhorse tool for testing Blazor apps is...</div>");
        });
    }

    [Fact]
    public void ShowsNotFoundWhenPostDoesNotExist()
    {
        var postId = Guid.NewGuid();
        _mockApi.GetPostByIdAsync(postId).Returns((BlogPost?)null);

        var cut = Render<PostDetail>(p => p
            .Add(pd => pd.PostId, postId)
        );

        cut.WaitForAssertion(() =>
        {
            Assert.NotNull(cut.Find(".not-found"));
            Assert.Throws<ElementNotFoundException>(() => cut.Find(".post-article"));
        });
    }

    [Fact]
    public void PublishedDateShowsAsVisibleText()
    {
        var postId = Guid.NewGuid();
        _mockApi.GetPostByIdAsync(postId).Returns(new BlogPost
        {
            Id = postId,
            Title = "A Post",
            Content = "Content",
            PublishedDate = new DateTime(2026, 3, 15),
            HeaderImageUrl = "https://example.com/test.jpg"
        });

        var cut = Render<PostDetail>(p => p
            .Add(pd => pd.PostId, postId)
        );

        cut.WaitForAssertion(() =>
            Assert.Equal("15 March 2026", cut.Find(".post-date").TextContent));
    }

    [Fact]
    public void HeaderImageHasCorrectAttributes()
    {
        var postId = Guid.NewGuid();
        _mockApi.GetPostByIdAsync(postId).Returns(new BlogPost
        {
            Id = postId,
            Title = "Introduction to Blazor",
            Content = "Content",
            HeaderImageUrl = "https://example.com/blazor-header.jpg"
        });

        var cut = Render<PostDetail>(p => p
            .Add(pd => pd.PostId, postId)
        );

        cut.WaitForAssertion(() =>
        {
            var img = cut.Find(".post-header-image");
            Assert.Equal("https://example.com/blazor-header.jpg", img.GetAttribute("src"));
            Assert.Equal("Introduction to Blazor", img.GetAttribute("alt"));
        });
    }

    // ── Module 4 Clip 2: Loading, error, and success states ──────────────────

    [Fact]
    public void ShowsLoadingStateWhileServiceCallIsPending()
    {
        var tcs = new TaskCompletionSource<BlogPost?>();
        var postId = Guid.NewGuid();
        _mockApi.GetPostByIdAsync(postId).Returns(tcs.Task);

        var cut = Render<PostDetail>(p => p
            .Add(pd => pd.PostId, postId)
        );

        // Service hasn't returned yet — the component should show loading
        Assert.NotNull(cut.Find(".loading"));
        Assert.Contains("Loading post...", cut.Find(".loading").TextContent);
        Assert.Throws<ElementNotFoundException>(() => cut.Find(".post-article"));
    }

    [Fact]
    public void ShowsErrorStateWhenServiceCallFails()
    {
        var tcs = new TaskCompletionSource<BlogPost?>();
        var postId = Guid.NewGuid();
        _mockApi.GetPostByIdAsync(postId).Returns(tcs.Task);

        var cut = Render<PostDetail>(p => p
            .Add(pd => pd.PostId, postId)
        );

        // Simulate a network error
        tcs.SetException(new HttpRequestException("Network error"));

        cut.WaitForAssertion(() =>
        {
            var errorDiv = cut.Find(".error-message");
            Assert.Contains("Network error", errorDiv.TextContent);
            Assert.Throws<ElementNotFoundException>(() => cut.Find(".loading"));
        });
    }

    [Fact]
    public void ShowsPostContentAfterSuccessfulLoad()
    {
        var tcs = new TaskCompletionSource<BlogPost?>();
        var postId = Guid.NewGuid();
        _mockApi.GetPostByIdAsync(postId).Returns(tcs.Task);

        var cut = Render<PostDetail>(p => p
            .Add(pd => pd.PostId, postId)
        );

        // Still loading
        Assert.NotNull(cut.Find(".loading"));

        // Service returns data
        tcs.SetResult(new BlogPost
        {
            Id = postId,
            Title = "Async Testing",
            Content = "TaskCompletionSource gives us full control.",
            PublishedDate = new DateTime(2026, 5, 1),
            HeaderImageUrl = "https://example.com/async.jpg"
        });

        // WaitForElement — blocks until the element appears in the DOM
        var article = cut.WaitForElement(".post-article");
        Assert.NotNull(article);

        Assert.Equal("Async Testing", cut.Find(".card-title").TextContent);
    }

    [Fact]
    public void TransitionsFromLoadingToNotFoundWhenPostIsNull()
    {
        var tcs = new TaskCompletionSource<BlogPost?>();
        var postId = Guid.NewGuid();
        _mockApi.GetPostByIdAsync(postId).Returns(tcs.Task);

        var cut = Render<PostDetail>(p => p
            .Add(pd => pd.PostId, postId)
        );

        Assert.NotNull(cut.Find(".loading"));

        // Service returns null — post doesn't exist
        tcs.SetResult(null);

        cut.WaitForAssertion(() =>
        {
            Assert.NotNull(cut.Find(".not-found"));
            Assert.Throws<ElementNotFoundException>(() => cut.Find(".loading"));
        });
    }

    // ── Module 5 Clip 1: Render mode tests ─────────────────────────────────

    [Fact]
    public void LikeButtonIsHiddenInStaticMode()
    {
        SetRendererInfo(new RendererInfo("Static", isInteractive: false));
        var postId = Guid.NewGuid();
        _mockApi.GetPostByIdAsync(postId).Returns(TestPost(postId));

        var cut = Render<PostDetail>(p => p.Add(pd => pd.PostId, postId));

        cut.WaitForAssertion(() =>
        {
            Assert.Throws<ElementNotFoundException>(() => cut.Find(".like-button"));
            Assert.NotNull(cut.Find(".like-static"));
        });
    }

    [Fact]
    public void LikeButtonIsVisibleInInteractiveMode()
    {
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        var postId = Guid.NewGuid();
        _mockApi.GetPostByIdAsync(postId).Returns(TestPost(postId));

        var cut = Render<PostDetail>(p => p.Add(pd => pd.PostId, postId));

        cut.WaitForAssertion(() =>
            Assert.NotNull(cut.Find(".like-button")));
    }

    [Fact]
    public void ClickingLikeIncrementsCount()
    {
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        var postId = Guid.NewGuid();
        _mockApi.GetPostByIdAsync(postId).Returns(TestPost(postId));

        var cut = Render<PostDetail>(p => p.Add(pd => pd.PostId, postId));

        cut.WaitForElement(".like-button");
        cut.Find(".like-button").Click();

        Assert.Contains("1", cut.Find(".like-button").TextContent);
    }

    // ── Module 5 Clip 1: JavaScript interop tests ─────────────────────────

    [Fact]
    public void LoadsSavedLikeCountFromLocalStorage()
    {
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        var postId = Guid.NewGuid();
        _mockApi.GetPostByIdAsync(postId).Returns(TestPost(postId));

        JSInterop.Setup<string>("localStorage.getItem", $"likes-{postId}")
            .SetResult("3");

        var cut = Render<PostDetail>(p => p.Add(pd => pd.PostId, postId));

        cut.WaitForAssertion(() =>
            Assert.Contains("3", cut.Find(".like-button").TextContent));
    }

    [Fact]
    public void ClickingLikeSavesToLocalStorage()
    {
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        var postId = Guid.NewGuid();
        _mockApi.GetPostByIdAsync(postId).Returns(TestPost(postId));

        JSInterop.Setup<string>("localStorage.getItem", $"likes-{postId}")
            .SetResult("0");

        var cut = Render<PostDetail>(p => p.Add(pd => pd.PostId, postId));

        cut.WaitForElement(".like-button");
        cut.Find(".like-button").Click();

        var invocations = JSInterop.VerifyInvoke("localStorage.setItem", calledTimes: 1);
        Assert.Equal($"likes-{postId}", invocations[0].Arguments[0]);
        Assert.Equal("1", invocations[0].Arguments[1]);
    }

    // ── Module 5 Clip 2: Authentication tests ─────────────────────────────

    [Fact]
    public void EditLinkIsHiddenForAnonymousUsers()
    {
        var postId = Guid.NewGuid();
        _mockApi.GetPostByIdAsync(postId).Returns(TestPost(postId));

        var cut = Render<PostDetail>(p => p.Add(pd => pd.PostId, postId));

        cut.WaitForAssertion(() =>
            Assert.Throws<ElementNotFoundException>(() => cut.Find(".edit-link")));
    }

    [Fact]
    public void EditLinkIsVisibleForAuthenticatedUsers()
    {
        _authContext.SetAuthorized("TestUser");
        var postId = Guid.NewGuid();
        _mockApi.GetPostByIdAsync(postId).Returns(TestPost(postId));

        var cut = Render<PostDetail>(p => p.Add(pd => pd.PostId, postId));

        cut.WaitForAssertion(() =>
            Assert.NotNull(cut.Find(".edit-link")));
    }

    private static BlogPost TestPost(Guid postId) => new()
    {
        Id = postId,
        Title = "Test Post",
        Content = "Test content",
        PublishedDate = new DateTime(2026, 1, 1),
        HeaderImageUrl = "https://example.com/test.jpg"
    };
}
