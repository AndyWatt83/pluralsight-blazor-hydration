using BlazorBloggingApp.Models;

namespace BlazorBloggingApp.Services;

public class InMemoryBlogApiClient : IBlogApiClient
{
    private readonly List<BlogPost> _posts =
    [
        new BlogPost
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Title = "Introduction to Blazor",
            Content = "Blazor is a framework for building interactive web UIs using C# instead of JavaScript.",
            PublishedDate = new DateTime(2026, 1, 15),
            HeaderImageUrl = "https://picsum.photos/seed/intro-blazor/800/300"
        },
        new BlogPost
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Title = "Testing with bUnit",
            Content = "Component testing gives you fast, focused feedback on your UI without spinning up a browser.",
            PublishedDate = new DateTime(2026, 3, 20),
            HeaderImageUrl = "https://picsum.photos/seed/bunit-testing/800/300"
        },
        new BlogPost
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Title = "State Management in Blazor",
            Content = "Managing state is a core concern in any web application. Blazor offers several patterns.",
            PublishedDate = new DateTime(2026, 2, 10),
            HeaderImageUrl = "https://picsum.photos/seed/state-management/800/300"
        }
    ];

    public async Task<BlogPost?> GetPostByIdAsync(Guid id)
    {
        await Task.Delay(1000); // simulate network latency
        return _posts.FirstOrDefault(p => p.Id == id);
    }

    public async Task<List<BlogPost>> GetAllPostsAsync()
    {
        return _posts.ToList();
    }

    public Task UpdatePostAsync(Guid id, string title, string content)
    {
        var post = _posts.FirstOrDefault(p => p.Id == id);
        if (post is not null)
        {
            post.Title = title;
            post.Content = content;
        }
        return Task.CompletedTask;
    }
}
