using BlazorBloggingApp.Components;
using System;

namespace BlazorBloggingApp.Tests;

public class PostSummaryTests : BunitContext
{
    [Fact]
    public void RendersTitleAndExcerpt()
    {
        var cut = Render<PostSummary>(p => p
            .Add(ps => ps.PostId, Guid.NewGuid())
            .Add(ps => ps.Title, "Introduction to Blazor")
            .Add(ps => ps.Excerpt, "Learn the basics of building web apps with Blazor...")
        );

        cut.Find(".post-summary-title").MarkupMatches(
            @"<h3 class=""post-summary-title card-title h6 mb-1"">Introduction to Blazor</h3>");

        Assert.Equal("Learn the basics of building web apps with Blazor...",
            cut.Find(".post-summary-excerpt").TextContent);
    }

    [Fact]
    public void LinksToThePostPage()
    {
        // The summary is a plain anchor, so navigation works under static SSR —
        // no event callback has to cross a render-mode boundary.
        var postId = Guid.NewGuid();

        var cut = Render<PostSummary>(p => p
            .Add(ps => ps.PostId, postId)
            .Add(ps => ps.Title, "Any Title")
            .Add(ps => ps.Excerpt, "Any excerpt")
        );

        Assert.Equal($"/post/{postId}",
            cut.Find(".post-summary").GetAttribute("href"));
    }
}
