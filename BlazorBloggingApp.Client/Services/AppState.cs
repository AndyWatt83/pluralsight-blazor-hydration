using Microsoft.AspNetCore.Components;

namespace BlazorBloggingApp.Services;

// Shared UI preferences: several components need the same value.
public class AppState
{
    [PersistentState]
    public string Theme { get; set; } = "light";

    public bool IsDark => Theme == "dark";
}
