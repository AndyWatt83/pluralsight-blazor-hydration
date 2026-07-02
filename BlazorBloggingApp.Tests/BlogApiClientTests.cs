using BlazorBloggingApp.Models;
using BlazorBloggingApp.Services;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorBloggingApp.Tests;

public class BlogApiClientTests
{
    private readonly MockHttpMessageHandler _mockHttp = new();

    private BlogApiClient CreateClient()
    {
        var httpClient = _mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:5001");
        return new BlogApiClient(httpClient);
    }

    [Fact]
    public async Task GetPostByIdAsync_ReturnsPost()
    {
        var postId = Guid.NewGuid();
        var expected = new BlogPost
        {
            Id = postId,
            Title = "Test Post",
            Content = "Test content",
            PublishedDate = new DateTime(2026, 3, 15),
            HeaderImageUrl = "https://example.com/test.jpg"
        };

        _mockHttp
            .When($"/api/posts/{postId}")
            .Respond("application/json", JsonSerializer.Serialize(expected));

        var client = CreateClient();

        var result = await client.GetPostByIdAsync(postId);

        Assert.NotNull(result);
        Assert.Equal("Test Post", result.Title);
        Assert.Equal("Test content", result.Content);
    }

    [Fact]
    public async Task GetAllPostsAsync_ReturnsList()
    {
        var posts = new List<BlogPost>
        {
            new() { Title = "First Post", Content = "First content" },
            new() { Title = "Second Post", Content = "Second content" }
        };

        _mockHttp
            .When("/api/posts")
            .Respond("application/json", JsonSerializer.Serialize(posts));

        var client = CreateClient();

        var result = await client.GetAllPostsAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("First Post", result[0].Title);
        Assert.Equal("Second Post", result[1].Title);
    }

    [Fact]
    public async Task UpdatePostAsync_SendsPutRequest()
    {
        var postId = Guid.NewGuid();

        _mockHttp
            .Expect(HttpMethod.Put, $"https://localhost:5001/api/posts/{postId}")
            .Respond(HttpStatusCode.NoContent);

        var client = CreateClient();

        await client.UpdatePostAsync(postId, "Updated Title", "Updated content");

        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetPostByIdAsync_ThrowsOnNetworkError()
    {
        var postId = Guid.NewGuid();

        _mockHttp
            .When($"/api/posts/{postId}")
            .Throw(new HttpRequestException("Connection refused"));

        var client = CreateClient();

        await Assert.ThrowsAsync<HttpRequestException>(
            () => client.GetPostByIdAsync(postId));
    }
}
