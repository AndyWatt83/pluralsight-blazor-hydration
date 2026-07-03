using BlazorBloggingApp.Components;
using BlazorBloggingApp.Models;
using BlazorBloggingApp.Services;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlazorBloggingApp.Tests;

public class PostListTests : BunitContext
{
    private readonly IBlogApiClient _mockApi = Substitute.For<IBlogApiClient>();

    public PostListTests()
    {
        // PostList is a self-contained interactive island: it fetches its own
        // posts rather than receiving them from the (static) layout.
        _mockApi.GetAllPostsAsync().Returns(SamplePosts());
        Services.AddSingleton(_mockApi);
    }

    private static List<BlogPost> SamplePosts() => new()
    {
        new BlogPost { Id = Guid.NewGuid(), Title = "Oldest", Content = "Content one",   PublishedDate = new DateTime(2024, 1, 1) },
        new BlogPost { Id = Guid.NewGuid(), Title = "Middle", Content = "Content two",   PublishedDate = new DateTime(2025, 1, 1) },
        new BlogPost { Id = Guid.NewGuid(), Title = "Newest", Content = "Content three", PublishedDate = new DateTime(2026, 1, 1) }
    };

    [Fact]
    public void FetchesItsOwnPostsAndRendersThem()
    {
        var cut = Render<PostList>();

        cut.WaitForAssertion(() =>
            Assert.Equal(3, cut.FindAll(".post-summary").Count));
    }

    [Fact]
    public void EachSummaryLinksToItsPost()
    {
        var posts = SamplePosts();
        _mockApi.GetAllPostsAsync().Returns(posts);
        var middlePost = posts.Single(p => p.Title == "Middle");

        var cut = Render<PostList>();

        // PostList defaults to newest-first, so position [1] is "Middle"
        cut.WaitForAssertion(() =>
            Assert.Equal($"/post/{middlePost.Id}",
                cut.FindAll(".post-summary")[1].GetAttribute("href")));
    }

    [Fact]
    public void TogglingSortReversesOrder()
    {
        var cut = Render<PostList>();

        // Default: newest first
        cut.WaitForAssertion(() =>
            Assert.Contains("Newest", cut.FindAll(".post-summary")[0].TextContent));

        // Toggle — now oldest first
        cut.Find(".post-list-sort").Click();
        Assert.Contains("Oldest", cut.FindAll(".post-summary")[0].TextContent);

        // Toggle back — newest first again
        cut.Find(".post-list-sort").Click();
        Assert.Contains("Newest", cut.FindAll(".post-summary")[0].TextContent);
    }

    [Fact]
    public void SortsNewestFirstByDefault()
    {
        var cut = Render<PostList>();

        cut.WaitForAssertion(() =>
        {
            var summaries = cut.FindAll(".post-summary");
            Assert.Contains("Newest", summaries[0].TextContent);
            Assert.Contains("Middle", summaries[1].TextContent);
            Assert.Contains("Oldest", summaries[2].TextContent);
        });
    }

    // ── Module 5 Clip 3 (testing course): Component stubbing tests ──────────

    [Fact]
    public void RendersCorrectNumberOfPostSummaries_WithStub()
    {
        ComponentFactories.AddStub<PostSummary>();

        var cut = Render<PostList>();

        cut.WaitForAssertion(() =>
            Assert.Equal(3, cut.FindComponents<Stub<PostSummary>>().Count));
    }

    [Fact]
    public void PassesCorrectTitleToPostSummary()
    {
        ComponentFactories.AddStub<PostSummary>();

        var cut = Render<PostList>();

        // Newest first by default — first stub should get "Newest"
        cut.WaitForAssertion(() =>
            Assert.Equal("Newest",
                cut.FindComponents<Stub<PostSummary>>()[0]
                    .Instance.Parameters.Get(ps => ps.Title)));
    }

    [Fact]
    public void RendersStubsWithCustomMarkup()
    {
        ComponentFactories.AddStub<PostSummary>(
            @"<div class=""stubbed-summary"">Stub</div>");

        var cut = Render<PostList>();

        cut.WaitForAssertion(() =>
            Assert.Equal(3, cut.FindAll(".stubbed-summary").Count));
    }
}
