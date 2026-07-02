using System.Net.Http.Json;
using BlazorBloggingApp.Models;

namespace BlazorBloggingApp.Services;

public class BlogApiClient(HttpClient http) : IBlogApiClient
{
    public async Task<BlogPost?> GetPostByIdAsync(Guid id)
        => await http.GetFromJsonAsync<BlogPost>($"api/posts/{id}");

    public async Task<List<BlogPost>> GetAllPostsAsync()
        => await http.GetFromJsonAsync<List<BlogPost>>("api/posts") ?? [];

    public async Task UpdatePostAsync(Guid id, string title, string content)
        => await http.PutAsJsonAsync($"api/posts/{id}", new { title, content });
}
