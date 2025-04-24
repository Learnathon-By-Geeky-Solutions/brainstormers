using Duende.IdentityServer.Models;
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
        public async Task GetClients_ShouldReturnExpectedClientConfiguration()
        {
            // Act
            var clients = IdentityServerConfig.GetClients().ToList();

            // Assert
            _output.WriteLine("Validating client configuration...");

            Assert.NotNull(clients);
            Assert.NotEmpty(clients);
            Assert.Single(clients);

            var client = clients[0];

            Assert.Equal("taskforge-client", client.ClientId);
            Assert.Equal(GrantTypes.Code, client.AllowedGrantTypes);
            Assert.True(client.RequirePkce);
            Assert.False(client.RequireClientSecret);
            Assert.Single(client.RedirectUris);
            Assert.Equal("https://localhost:5001/signin-oidc", client.RedirectUris.Single());
            Assert.Single(client.PostLogoutRedirectUris);
            Assert.Equal("https://localhost:5001/signout-callback-oidc", client.PostLogoutRedirectUris.Single());

            var expectedScopes = new[] { "openid", "profile", "api1" };
            Assert.Equal(expectedScopes.Length, client.AllowedScopes.Count);
            foreach (var scope in expectedScopes)
            {
                Assert.Contains(scope, client.AllowedScopes);
            }

            _output.WriteLine("Client configuration validated successfully.");
            await Task.CompletedTask;
        }

        [Fact]
        public async Task GetApiScopes_ShouldReturnExpectedApiScope()
        {
            // Act
            var apiScopes = IdentityServerConfig.GetApiScopes().ToList();

            // Assert
            _output.WriteLine("Validating API scopes...");

            Assert.NotNull(apiScopes);
            Assert.NotEmpty(apiScopes);
            Assert.Single(apiScopes);

            var scope = apiScopes[0];

            Assert.Equal("api1", scope.Name);
            Assert.Equal("TaskForge API", scope.DisplayName);

            _output.WriteLine("API scope validated successfully.");
            await Task.CompletedTask;
        }
    }
}
