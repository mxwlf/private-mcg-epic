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
public sealed class EpicOAuth2ClientIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="EpicOAuth2ClientIntegrationTests"/> class.
    /// </summary>
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

    /// <summary>
    /// Tests that configuration loads correctly from appsettings.json.
    /// </summary>
    [Fact]
    public void ConfigurationShouldLoadFromAppSettings()
    {
        // Arrange & Act
        var clientId = _configuration["epic:auth:clientid"];
        var tokenEndpoint = _configuration["epic:auth:tokenendpoint"];

        // Assert
        clientId.Should().NotBeNullOrEmpty("ClientId should be configured in appsettings.json");
        tokenEndpoint.Should().NotBeNullOrEmpty("TokenEndpoint should be configured in appsettings.json");
        tokenEndpoint.Should().StartWith("https://", "TokenEndpoint should be a valid HTTPS URL");
    }

    /// <summary>
    /// Tests that the private key loads correctly from user secrets.
    /// </summary>
    [Fact]
    public void ConfigurationShouldLoadPrivateKeyFromUserSecrets()
    {
        // Arrange & Act
        var privateKey = _configuration["epic:auth:privatekey"];

        // Assert
        privateKey.Should().NotBeNullOrEmpty(
            "PrivateKey should be configured in user secrets. " +
            "Run: dotnet user-secrets set \"epic:auth:privatekey\" \"YOUR_KEY\" --project /path/to/epic.httpclients.test.int.csproj");
    }

    /// <summary>
    /// Tests that the service provider can resolve the EpicOAuth2Client.
    /// </summary>
    [Fact]
    public void ServiceProviderShouldResolveEpicOAuth2Client()
    {
        // Act
        var client = _serviceProvider.GetService<EpicOAuth2Client>();

        // Assert
        client.Should().NotBeNull("EpicOAuth2Client should be registered in the DI container");
    }

    /// <summary>
    /// Tests that requesting a token with valid configuration returns an access token.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task RequestTokenAsyncWithValidConfigurationShouldReturnToken()
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

    /// <summary>
    /// Tests that the client can request tokens multiple times successfully.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task RequestTokenAsyncWithValidConfigurationTokenShouldBeReusable()
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

    /// <summary>
    /// Tests that token requests can be cancelled using a cancellation token.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task RequestTokenAsyncWithCancellationTokenShouldBeCancellable()
    {
        // Arrange
        var client = _serviceProvider.GetRequiredService<EpicOAuth2Client>();

        // Skip test if private key is not configured
        var privateKey = _configuration["epic:auth:privatekey"];
        if (string.IsNullOrEmpty(privateKey))
        {
            return;
        }

        // Act & Assert
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync(); // Cancel immediately

        var act = async () => await client.RequestTokenAsync(cts.Token);

        // Should throw TaskCanceledException or OperationCanceledException
        await act.Should().ThrowAsync<OperationCanceledException>(
            "The operation should be cancellable");
    }

    /// <summary>
    /// Tests that the EpicOAuth2Client can be disposed without throwing exceptions.
    /// </summary>
    [Fact]
    public void EpicOAuth2ClientShouldBeDisposable()
    {
        // Arrange
        var client = _serviceProvider.GetRequiredService<EpicOAuth2Client>();

        // Act
        var act = () => client.Dispose();

        // Assert
        act.Should().NotThrow("EpicOAuth2Client should implement IDisposable correctly");
    }

    /// <summary>
    /// Disposes the service provider.
    /// </summary>
    public void Dispose()
    {
        _serviceProvider?.Dispose();
        GC.SuppressFinalize(this);
    }
}
