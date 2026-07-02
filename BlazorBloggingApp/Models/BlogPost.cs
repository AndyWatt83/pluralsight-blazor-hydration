namespace BlazorBloggingApp.Models;

public class BlogPost
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTime PublishedDate { get; set; } = DateTime.Now;
    public string HeaderImageUrl { get; set; } = "https://picsum.photos/seed/blazor/800/300";
}
