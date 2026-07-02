using BlazorBloggingApp.Components;
using BlazorBloggingApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlazorBloggingApp.Tests;

public class PostListTests : BunitContext
{
    private static List<BlogPost> SamplePosts() => new()
    {
        new BlogPost { Id = Guid.NewGuid(), Title = "Oldest", Content = "Content one",   PublishedDate = new DateTime(2024, 1, 1) },
        new BlogPost { Id = Guid.NewGuid(), Title = "Middle", Content = "Content two",   PublishedDate = new DateTime(2025, 1, 1) },
        new BlogPost { Id = Guid.NewGuid(), Title = "Newest", Content = "Content three", PublishedDate = new DateTime(2026, 1, 1) }
    };

    [Fact]
    public void ClickingAPostSummaryFiresOnPostSelected()
    {
        var posts = SamplePosts();
        var middlePost = posts.Single(p => p.Title == "Middle");
        Guid? receivedId = null;

        var cut = Render<PostList>(p => p
            .Add(pl => pl.Posts, posts)
            .Add(pl => pl.OnPostSelected, id => receivedId = id)
        );

        // PostList defaults to newest-first, so position [1] is "Middle"
        cut.FindAll(".post-summary")[1].Click();

        Assert.Equal(middlePost.Id, receivedId);
    }

    [Fact]
    public void TogglingSortReversesOrder()
    {
        var cut = Render<PostList>(p => p.Add(pl => pl.Posts, SamplePosts()));

        // Default: newest first
        Assert.Contains("Newest", cut.FindAll(".post-summary")[0].TextContent);

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
        var cut = Render<PostList>(p => p.Add(pl => pl.Posts, SamplePosts()));

        var summaries = cut.FindAll(".post-summary");
        Assert.Contains("Newest", summaries[0].TextContent);
        Assert.Contains("Middle", summaries[1].TextContent);
        Assert.Contains("Oldest", summaries[2].TextContent);
    }

    [Fact]
    public void RendersOnePostSummaryPerPost()
    {
        var cut = Render<PostList>(p => p.Add(pl => pl.Posts, SamplePosts()));

        Assert.Equal(3, cut.FindAll(".post-summary").Count);
    }

    // ── Module 5 Clip 3: Component stubbing tests ───────────────────────────

    [Fact]
    public void RendersCorrectNumberOfPostSummaries_WithStub()
    {
        ComponentFactories.AddStub<PostSummary>();

        var cut = Render<PostList>(p => p.Add(pl => pl.Posts, SamplePosts()));

        var stubs = cut.FindComponents<Stub<PostSummary>>();
        Assert.Equal(3, stubs.Count);
    }

    [Fact]
    public void PassesCorrectTitleToPostSummary()
    {
        ComponentFactories.AddStub<PostSummary>();

        var posts = SamplePosts();
        var cut = Render<PostList>(p => p.Add(pl => pl.Posts, posts));

        // Newest first by default — first stub should get "Newest"
        var firstStub = cut.FindComponents<Stub<PostSummary>>()[0];
        Assert.Equal("Newest",
            firstStub.Instance.Parameters.Get(ps => ps.Title));
    }

    [Fact]
    public void RendersStubsWithCustomMarkup()
    {
        ComponentFactories.AddStub<PostSummary>(
            @"<div class=""stubbed-summary"">Stub</div>");

        var cut = Render<PostList>(p => p.Add(pl => pl.Posts, SamplePosts()));

        Assert.Equal(3, cut.FindAll(".stubbed-summary").Count);
    }
}
