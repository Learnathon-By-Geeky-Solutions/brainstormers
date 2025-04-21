using Duende.IdentityServer.Models;
using FluentAssertions;
using TaskForge.Infrastructure;
using Xunit;

namespace TaskForge.Tests.Infrastructure
{
    public class IdentityServerConfigTests
    {
        public IdentityServerConfigTests()
        {
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }

        [Fact]
        public void GetClients_ShouldReturnExpectedClientConfiguration()
        {
            var clients = IdentityServerConfig.GetClients();

            clients.Should().NotBeNullOrEmpty();
            var client = clients.Should().ContainSingle().Subject;

            client.ClientId.Should().Be("taskforge-client");
            client.AllowedGrantTypes.Should().BeEquivalentTo(GrantTypes.Code);
            client.RequirePkce.Should().BeTrue();
            client.RequireClientSecret.Should().BeFalse();
            client.RedirectUris.Should().ContainSingle().Which.Should().Be("https://localhost:5001/signin-oidc");
            client.PostLogoutRedirectUris.Should().ContainSingle().Which.Should().Be("https://localhost:5001/signout-callback-oidc");
            client.AllowedScopes.Should().BeEquivalentTo("openid", "profile", "api1");
        }

        [Fact]
        public void GetApiScopes_ShouldReturnExpectedApiScope()
        {
            var apiScopes = IdentityServerConfig.GetApiScopes();

            apiScopes.Should().NotBeNullOrEmpty();
            var scope = apiScopes.Should().ContainSingle().Subject;

            scope.Name.Should().Be("api1");
            scope.DisplayName.Should().Be("TaskForge API");
        }
    }
}
