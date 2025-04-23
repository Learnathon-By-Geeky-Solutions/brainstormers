using Duende.IdentityServer.Models;
using FluentAssertions;
using TaskForge.Infrastructure;
using Xunit;
using Xunit.Abstractions;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace TaskForge.Tests.Infrastructure
{
    public class IdentityServerConfigTests
    {
        private readonly ITestOutputHelper _output;

        static IdentityServerConfigTests()
        {
            // Patch Console globally before any tests run
            EnsureConsoleStreamsOpenStatic();
        }

        public IdentityServerConfigTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private static void EnsureConsoleStreamsOpenStatic()
        {
            if (Console.Out == TextWriter.Null || Console.Out is StreamWriter { BaseStream.CanWrite: false })
            {
                Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
            }

            if (Console.Error == TextWriter.Null || Console.Error is StreamWriter { BaseStream.CanWrite: false })
            {
                Console.SetError(new StreamWriter(Console.OpenStandardError()) { AutoFlush = true });
            }
        }

        [Fact]
        public void GetClients_ShouldReturnExpectedClientConfiguration()
        {
            // Act
            var clients = IdentityServerConfig.GetClients();

            // Assert
            _output.WriteLine("Validating client configuration...");

            clients.Should().NotBeNullOrEmpty("at least one client should be configured");
            var client = clients.Should().ContainSingle().Subject;

            client.ClientId.Should().Be("taskforge-client");
            client.AllowedGrantTypes.Should().BeEquivalentTo(GrantTypes.Code);
            client.RequirePkce.Should().BeTrue();
            client.RequireClientSecret.Should().BeFalse();
            client.RedirectUris.Should().ContainSingle().Which.Should().Be("https://localhost:5001/signin-oidc");
            client.PostLogoutRedirectUris.Should().ContainSingle().Which.Should().Be("https://localhost:5001/signout-callback-oidc");
            client.AllowedScopes.Should().BeEquivalentTo("openid", "profile", "api1");

            _output.WriteLine("Client configuration validated successfully.");
        }

        [Fact]
        public void GetApiScopes_ShouldReturnExpectedApiScope()
        {
            // Act
            var apiScopes = IdentityServerConfig.GetApiScopes();

            // Assert
            _output.WriteLine("Validating API scopes...");

            apiScopes.Should().NotBeNullOrEmpty("at least one API scope should be configured");
            var scope = apiScopes.Should().ContainSingle().Subject;

            scope.Name.Should().Be("api1");
            scope.DisplayName.Should().Be("TaskForge API");

            _output.WriteLine("API scope validated successfully.");
        }
    }
}
