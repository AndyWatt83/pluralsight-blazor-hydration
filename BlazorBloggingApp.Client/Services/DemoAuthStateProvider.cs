using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorBloggingApp.Services;

public class DemoAuthStateProvider : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, "Demo User"),
            new Claim(ClaimTypes.Role, "Admin")
        ], "DemoAuth");

        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }
}
