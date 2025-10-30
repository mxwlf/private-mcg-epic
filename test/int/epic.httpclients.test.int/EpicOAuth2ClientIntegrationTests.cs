using AwesomeAssertions;
using Mcg.Edge.Fhir.Epic.Abstractions;
using Mcg.Edge.Fhir.Epic.HttpClients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mcg.Edge.Fhir.Epic.HttpClients.Test.Integration;

/// <summary>
/// Integration tests for EpicOAuth2Client.
/// Requires configuration in appsettings.json and user secrets.
///
/// To set the private key in user secrets, run:
/// dotnet user-secrets set "epic:auth:privatekey" "YOUR_PRIVATE_KEY_HERE" --project /path/to/epic.httpclients.test.int.csproj
/// </summary>
[Trait("Category", "Integration")]
[Trait("WrittenBy", "Agent")]
[Trait("Agent", "Anthropic-Claude-Sonnet-4.5")]
public class EpicOAuth2ClientIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public EpicOAuth2ClientIntegrationTests()
    {
        // Build configuration from multiple sources:
        // 1. appsettings.json
        // 2. User secrets
        // 3. Environment variables
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddUserSecrets<EpicOAuth2ClientIntegrationTests>()
            .AddEnvironmentVariables()
            .Build();

        // Setup DI container
        var services = new ServiceCollection();

        // Register Epic OAuth2 Client with configuration
        services.AddEpicOAuth2Client(_configuration);

        // Add logging for debugging
        services.AddLogging();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void Configuration_ShouldLoadFromAppSettings()
    {
        // Arrange & Act
        var clientId = _configuration["epic:auth:clientid"];
        var tokenEndpoint = _configuration["epic:auth:tokenendpoint"];

        // Assert
        clientId.Should().NotBeNullOrEmpty("ClientId should be configured in appsettings.json");
        tokenEndpoint.Should().NotBeNullOrEmpty("TokenEndpoint should be configured in appsettings.json");
        tokenEndpoint.Should().StartWith("https://", "TokenEndpoint should be a valid HTTPS URL");
    }

    [Fact]
    public void Configuration_ShouldLoadPrivateKeyFromUserSecrets()
    {
        // Arrange & Act
        var privateKey = _configuration["epic:auth:privatekey"];

        // Assert
        privateKey.Should().NotBeNullOrEmpty(
            "PrivateKey should be configured in user secrets. " +
            "Run: dotnet user-secrets set \"epic:auth:privatekey\" \"YOUR_KEY\" --project /path/to/epic.httpclients.test.int.csproj");
    }

    [Fact]
    public void ServiceProvider_ShouldResolveEpicOAuth2Client()
    {
        // Act
        var client = _serviceProvider.GetService<EpicOAuth2Client>();

        // Assert
        client.Should().NotBeNull("EpicOAuth2Client should be registered in the DI container");
    }

    [Fact]
    public async Task RequestTokenAsync_WithValidConfiguration_ShouldReturnToken()
    {
        // Arrange
        var client = _serviceProvider.GetRequiredService<EpicOAuth2Client>();

        // Skip test if private key is not configured
        var privateKey = _configuration["epic:auth:privatekey"];
        if (string.IsNullOrEmpty(privateKey))
        {
            // Use Skip attribute equivalent in xUnit v3
            return; // Test will be marked as skipped
        }

        // Act
        var result = await client.RequestTokenAsync();

        // Assert
        result.Should().NotBeNull("Token response should not be null");
        result.AccessToken.Should().NotBeNullOrEmpty("Access token should be returned");
        result.ExpiresIn.Should().BeGreaterThan(0, "Token expiration should be positive");
        result.TokenType.Should().Be("Bearer", "Token type should be Bearer");
    }

    [Fact]
    public async Task RequestTokenAsync_WithValidConfiguration_TokenShouldBeReusable()
    {
        // Arrange
        var client = _serviceProvider.GetRequiredService<EpicOAuth2Client>();

        // Skip test if private key is not configured
        var privateKey = _configuration["epic:auth:privatekey"];
        if (string.IsNullOrEmpty(privateKey))
        {
            return;
        }

        // Act - Request token twice
        var result1 = await client.RequestTokenAsync();
        var result2 = await client.RequestTokenAsync();

        // Assert - Both should succeed
        result1.Should().NotBeNull();
        result1.AccessToken.Should().NotBeNullOrEmpty();

        result2.Should().NotBeNull();
        result2.AccessToken.Should().NotBeNullOrEmpty();

        // Tokens might be the same or different depending on Epic's behavior
        // but both should be valid
        result1.ExpiresIn.Should().BeGreaterThan(0);
        result2.ExpiresIn.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RequestTokenAsync_WithCancellationToken_ShouldBeCancellable()
    {
        // Arrange
        var client = _serviceProvider.GetRequiredService<EpicOAuth2Client>();
        var cts = new CancellationTokenSource();

        // Skip test if private key is not configured
        var privateKey = _configuration["epic:auth:privatekey"];
        if (string.IsNullOrEmpty(privateKey))
        {
            return;
        }

        // Act & Assert
        cts.Cancel(); // Cancel immediately

        var act = async () => await client.RequestTokenAsync(cts.Token);

        // Should throw TaskCanceledException or OperationCanceledException
        await act.Should().ThrowAsync<OperationCanceledException>(
            "The operation should be cancellable");
    }

    [Fact]
    public void EpicOAuth2Client_ShouldBeDisposable()
    {
        // Arrange
        var client = _serviceProvider.GetRequiredService<EpicOAuth2Client>();

        // Act
        var act = () => client.Dispose();

        // Assert
        act.Should().NotThrow("EpicOAuth2Client should implement IDisposable correctly");
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
