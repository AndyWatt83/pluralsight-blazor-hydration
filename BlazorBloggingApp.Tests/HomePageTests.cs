using BlazorBloggingApp.Components.Pages;

namespace BlazorBloggingApp.Tests;

public class HomePageTests : BunitContext
{
    [Fact]
    public void Home_Displays_Blog_Heading()
    {
        var cut = Render<Home>();
        cut.Find("h1").MarkupMatches("<h1>Blazor Blogging</h1>");
    }
}
