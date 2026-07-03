using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BlazorBloggingApp.Tests.LearningOutcomes;

/// <summary>
/// Boots the real app (full pipeline, real InMemoryBlogApiClient) so
/// learning-outcome tests can assert on genuine prerendered HTML — the same
/// bytes a browser's view-source shows on camera.
/// </summary>
public class BlogAppFixture : WebApplicationFactory<Program>
{
    /// <summary>The seeded "Introduction to Blazor" post.</summary>
    public static readonly Guid IntroPostId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public async Task<string> GetPrerenderedHtmlAsync(string path)
    {
        using var client = CreateClient();
        var response = await client.GetAsync(path);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
