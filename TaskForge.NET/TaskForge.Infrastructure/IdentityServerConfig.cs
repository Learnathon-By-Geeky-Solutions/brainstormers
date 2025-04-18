using Duende.IdentityServer.Models;

namespace TaskForge.Infrastructure
{
    public static class IdentityServerConfig
    {
        public static IEnumerable<Client> GetClients() => new List<Client>
        {
            new Client
            {
                ClientId = "taskforge-client",
                AllowedGrantTypes = GrantTypes.Code,
                RequirePkce = true,
                RequireClientSecret = false,
                RedirectUris = { "https://localhost:5001/signin-oidc" },
                PostLogoutRedirectUris = { "https://localhost:5001/signout-callback-oidc" },
                AllowedScopes = { "openid", "profile", "api1" }
            }
        };

        public static IEnumerable<ApiScope> GetApiScopes() => new List<ApiScope>
        {
            new ApiScope("api1", "TaskForge API")
        };
    }

}
