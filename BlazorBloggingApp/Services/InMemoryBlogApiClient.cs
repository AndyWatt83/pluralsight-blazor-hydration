using BlazorBloggingApp.Models;

namespace BlazorBloggingApp.Services;

public class InMemoryBlogApiClient(
    ILogger<InMemoryBlogApiClient> logger,
    IHttpContextAccessor httpContextAccessor) : IBlogApiClient
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
        logger.LogInformation("Fetching post {PostId} [{Caller}]", id, DescribeCaller());

        // simulated latency for interactive call — long enough to see a fetch happen
        if (IsInteractiveCaller()) await Task.Delay(2000); 

        return _posts.FirstOrDefault(p => p.Id == id);
    }





    public async Task<List<BlogPost>> GetAllPostsAsync()
    {
        logger.LogInformation("Fetching all posts [{Caller}]", DescribeCaller());
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

    // Diagnostic only. This singleton can't know who called it, so it infers the
    // render path from the ambient HTTP request: none (or the /_blazor socket) is
    // the circuit, /api is the WebAssembly client's HTTP call, anything else is a
    // server render of a page. Enhanced navigation fetches send this Accept value.
    private string DescribeCaller()
    {
        var request = httpContextAccessor.HttpContext?.Request;
        if (request is null || request.Path.StartsWithSegments("/_blazor"))
        {
            return "SignalR circuit";
        }

        if (request.Path.StartsWithSegments("/api"))
        {
            return $"WASM via HTTP {request.Method} {request.Path}";
        }

        var enhancedNav = request.Headers.Accept.ToString()
            .Contains("blazor-enhanced-nav=on", StringComparison.OrdinalIgnoreCase);
        return $"SSR {(enhancedNav ? "enhanced nav" : "full load")} {request.Path}";
    }

    // Same inference as DescribeCaller: the circuit (Server) or the /api call
    // (WebAssembly) means an interactive component is fetching.
    private bool IsInteractiveCaller()
    {
        var request = httpContextAccessor.HttpContext?.Request;
        return request is null
            || request.Path.StartsWithSegments("/_blazor")
            || request.Path.StartsWithSegments("/api");
    }
}
