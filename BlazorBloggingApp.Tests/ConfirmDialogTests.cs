using BlazorBloggingApp.Components;

namespace BlazorBloggingApp.Tests;

public class ConfirmDialogTests : BunitContext
{
    [Fact]
    public void RendersNothingWhenNotVisible()
    {
        var cut = Render<ConfirmDialog>(p => p
            .Add(d => d.IsVisible, false)
            .Add(d => d.Message, "Are you sure?")
        );

        Assert.Empty(cut.FindAll(".confirm-dialog"));
    }

    [Fact]
    public void ShowsMessageWhenVisible()
    {
        var cut = Render<ConfirmDialog>(p => p
            .Add(d => d.IsVisible, true)
            .Add(d => d.Message, "This post is published. Are you sure?")
        );

        Assert.Equal("This post is published. Are you sure?",
            cut.Find(".confirm-message").TextContent);
    }

    // The headline test: the upward half of two-way binding.
    [Fact]
    public void CancellingClosesTheDialog()
    {
        var isVisible = true;

        var cut = Render<ConfirmDialog>(p => p
            .Bind(d => d.IsVisible, isVisible, v => isVisible = v)
            .Add(d => d.Message, "This post is published. Are you sure?")
        );

        cut.Find(".confirm-no").Click();

        Assert.False(isVisible);   // the parent's copy received the change
    }

    [Fact]
    public void ConfirmingFiresOnConfirmedAndCloses()
    {
        var isVisible = true;
        var confirmed = false;

        var cut = Render<ConfirmDialog>(p => p
            .Bind(d => d.IsVisible, isVisible, v => isVisible = v)
            .Add(d => d.OnConfirmed, () => confirmed = true)
            .Add(d => d.Message, "This post is published. Are you sure?")
        );

        cut.Find(".confirm-yes").Click();

        Assert.True(confirmed);
        Assert.False(isVisible);
    }
}
