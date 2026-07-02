using BlazorBloggingApp.Components.Pages;
using BlazorBloggingApp.Models;
using BlazorBloggingApp.Services;
using Microsoft.AspNetCore.Components;
using NSubstitute;
using System;
using System.Threading.Tasks;

namespace BlazorBloggingApp.Tests;

public class EditPostTests : BunitContext
{
    private readonly IBlogApiClient _mockApi = Substitute.For<IBlogApiClient>();

    public EditPostTests()
    {
        Services.AddSingleton(_mockApi);
    }

    [Fact]
    public void RendersFormWithPostData()
    {
        var postId = Guid.NewGuid();
        _mockApi.GetPostByIdAsync(postId).Returns(new BlogPost
        {
            Id = postId,
            Title = "Original Title",
            Content = "Original content"
        });

        var cut = Render<EditPost>(p => p
            .Add(ep => ep.PostId, postId)
        );

        cut.WaitForAssertion(() =>
        {
            var titleInput = cut.Find(".title-input");
            var contentInput = cut.Find(".content-input");

            Assert.Equal("Original Title", titleInput.GetAttribute("value"));
            Assert.Equal("Original content", contentInput.GetAttribute("value"));
        });
    }

    [Fact]
    public void SubmittingTheFormCallsUpdateAndNavigates()
    {
        var postId = Guid.NewGuid();
        _mockApi.GetPostByIdAsync(postId).Returns(new BlogPost
        {
            Id = postId,
            Title = "Original Title",
            Content = "Original content"
        });
        _mockApi.UpdatePostAsync(default, default!, default!).ReturnsForAnyArgs(Task.CompletedTask);

        var cut = Render<EditPost>(p => p
            .Add(ep => ep.PostId, postId)
        );

        cut.WaitForElement(".title-input");

        // Change the title
        cut.Find(".title-input").Change("A New Title");

        // Submit the form
        cut.Find("form").Submit();

        // Confirm the dialog
        cut.Find(".confirm-yes").Click();

        // Verify the service was called with the right arguments
        _mockApi.Received(1).UpdatePostAsync(postId, "A New Title", "Original content");

        // Verify navigation to the post detail page
        var nav = Services.GetRequiredService<NavigationManager>();
        Assert.EndsWith($"/post/{postId}", nav.Uri);
    }

    [Fact]
    public void ShowsValidationMessageWhenTitleIsMissing()
    {
        var postId = Guid.NewGuid();
        _mockApi.GetPostByIdAsync(postId).Returns(new BlogPost
        {
            Id = postId,
            Title = "",
            Content = "Some content"
        });

        var cut = Render<EditPost>(p => p
            .Add(ep => ep.PostId, postId)
        );

        cut.WaitForElement("form");

        cut.Find("form").Submit();

        Assert.Contains("Title is required",
            cut.Find(".validation-message").TextContent);
    }

    [Fact]
    public void CancellingTheDialogDoesNotCallService()
    {
        var postId = Guid.NewGuid();
        _mockApi.GetPostByIdAsync(postId).Returns(new BlogPost
        {
            Id = postId,
            Title = "Original Title",
            Content = "Original content"
        });

        var cut = Render<EditPost>(p => p
            .Add(ep => ep.PostId, postId)
        );

        cut.WaitForElement(".title-input");

        cut.Find(".title-input").Change("Changed Title");
        cut.Find("form").Submit();

        // Cancel instead of confirming
        cut.Find(".confirm-no").Click();

        // Service should NOT have been called
        _mockApi.DidNotReceive().UpdatePostAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>());
    }
}
