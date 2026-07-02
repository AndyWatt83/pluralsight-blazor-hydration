using BlazorBloggingApp.Models;

namespace BlazorBloggingApp.Services;

public interface IBlogApiClient
{
    Task<BlogPost?> GetPostByIdAsync(Guid id);
    Task<List<BlogPost>> GetAllPostsAsync();
    Task UpdatePostAsync(Guid id, string title, string content);
}
