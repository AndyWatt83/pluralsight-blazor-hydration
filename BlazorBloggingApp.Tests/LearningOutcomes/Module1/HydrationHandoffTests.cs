using System.Threading.Tasks;

namespace BlazorBloggingApp.Tests.LearningOutcomes.Module1;

/// <summary>
/// Module 1 — The Hydration Handoff (TO1).
/// The server renders the post during prerendering, so the data is already in
/// the HTML that reaches the browser. The "fetches twice, nothing carries over"
/// characterization tests describe the Module 1 code (tag m1-end); Module 2
/// fixes the bug, so they now live in Module2/PersistedStateTests.cs, flipped.
/// </summary>
public class HydrationHandoffTests : IClassFixture<BlogAppFixture>
{
    private readonly BlogAppFixture _app;

    public HydrationHandoffTests(BlogAppFixture app) => _app = app;

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
}
