using AwesomeAssertions;
using Mcg.Edge.Fhir.Epic.Abstractions;
using Mcg.Edge.Fhir.Epic.HttpClients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mcg.Edge.Fhir.Epic.HttpClients.Test.Integration;

/// <summary>
/// Integration tests for EpicMedicationsHttpClient.
/// Tests real API calls to Epic's medication endpoints using OAuth2 authentication.
/// Requires configuration in appsettings.json and user secrets.
/// </summary>
[Trait("Category", "Integration")]
[Trait("WrittenBy", "Agent")]
[Trait("Agent", "Anthropic-Claude-Sonnet-4.5")]
public sealed class EpicMedicationsHttpClientIntegrationTests : IDisposable
{
    private const string BaseUrl = "https://vendorservices.epic.com/interconnect-amcurprd-oauth";
    private const string PatientId = "eYg3-1aJmCMq-umIIq2Njxw3";

    private readonly ServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="EpicMedicationsHttpClientIntegrationTests"/> class.
    /// </summary>
    public EpicMedicationsHttpClientIntegrationTests()
    {
        // Build configuration from multiple sources:
        // 1. appsettings.json
        // 2. User secrets
        // 3. Environment variables
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddUserSecrets<EpicMedicationsHttpClientIntegrationTests>()
            .AddEnvironmentVariables()
            .Build();

        // Setup DI container
        var services = new ServiceCollection();

        // Register Epic OAuth2 Client for authentication
        services.AddEpicOAuth2Client(_configuration);

        // Register Epic Medications Client
        services.AddEpicMedicationsClient(_configuration);

        // Add logging for debugging
        services.AddLogging();

        _serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Tests that the medications client can be resolved from DI.
    /// </summary>
    [Fact]
    public void ServiceProviderShouldResolveEpicMedicationsClient()
    {
        // Act
        var client = _serviceProvider.GetService<IEpicCurrentMedicationsClient>();

        // Assert
        client.Should().NotBeNull("IEpicCurrentMedicationsClient should be registered in the DI container");
    }

    /// <summary>
    /// Tests that the OAuth2 client can be resolved from DI.
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
    /// Tests retrieving current medications for a patient using real Epic API.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetCurrentMedicationsAsyncShouldReturnMedicationsForPatient()
    {
        // Arrange
        var privateKey = _configuration["epic:auth:privatekey"];
        if (string.IsNullOrEmpty(privateKey))
        {
            // Skip test if private key is not configured
            return;
        }

        var oauthClient = _serviceProvider.GetRequiredService<EpicOAuth2Client>();
        var medicationsClient = _serviceProvider.GetRequiredService<IEpicCurrentMedicationsClient>();

        // Get OAuth2 token
        var tokenResponse = await oauthClient.RequestTokenAsync();
        tokenResponse.AccessToken.Should().NotBeNullOrEmpty("OAuth2 token should be obtained");

        var request = new CurrentMedicationsRequest
        {
            PatientId = PatientId,
            PatientIdType = "FHIR",
            ProfileView = 2,
            NumberDaysToIncludeDiscontinuedAndEndedOrders = 30
        };

        // Act
        var result = await medicationsClient.GetCurrentMedicationsAsync(
            BaseUrl,
            request,
            tokenResponse.AccessToken);

        // Assert
        result.Should().NotBeNullOrEmpty("Response should not be empty");

        // The response should be valid JSON
        var isValidJson = IsValidJson(result);
        isValidJson.Should().BeTrue("Response should be valid JSON");
    }

    /// <summary>
    /// Tests retrieving medication administration history for a patient using real Epic API.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetMedicationAdministrationHistoryShouldReturnHistoryForPatient()
    {
        // Arrange
        var privateKey = _configuration["epic:auth:privatekey"];
        if (string.IsNullOrEmpty(privateKey))
        {
            // Skip test if private key is not configured
            return;
        }

        var oauthClient = _serviceProvider.GetRequiredService<EpicOAuth2Client>();
        var medicationsClient = _serviceProvider.GetRequiredService<IEpicCurrentMedicationsClient>();

        // Get OAuth2 token
        var tokenResponse = await oauthClient.RequestTokenAsync();
        tokenResponse.AccessToken.Should().NotBeNullOrEmpty("OAuth2 token should be obtained");

        var request = new MedicationAdministrationRequest
        {
            PatientId = PatientId,
            PatientIdType = "FHIR",
            ContactId = "12345",  // This might need to be a real contact ID from your Epic instance
            ContactIdType = "CSN",
            OrderIds = new List<IdType>
            {
                new IdType { Id = "1", Type = "Internal" }
            }
        };

        // Act
        var result = await medicationsClient.GetCurrentMedicationsHistoryAsync(
            BaseUrl,
            request,
            tokenResponse.AccessToken);

        // Assert
        result.Should().NotBeNullOrEmpty("Response should not be empty");

        // The response should be valid JSON
        var isValidJson = IsValidJson(result);
        isValidJson.Should().BeTrue("Response should be valid JSON");
    }

    /// <summary>
    /// Tests that GetCurrentMedications handles authentication properly with the bearer token.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetCurrentMedicationsAsyncShouldUseProperAuthentication()
    {
        // Arrange
        var privateKey = _configuration["epic:auth:privatekey"];
        if (string.IsNullOrEmpty(privateKey))
        {
            return;
        }

        var oauthClient = _serviceProvider.GetRequiredService<EpicOAuth2Client>();
        var medicationsClient = _serviceProvider.GetRequiredService<IEpicCurrentMedicationsClient>();

        var tokenResponse = await oauthClient.RequestTokenAsync();

        var request = new CurrentMedicationsRequest
        {
            PatientId = PatientId,
            PatientIdType = "FHIR",
            ProfileView = 2,
            NumberDaysToIncludeDiscontinuedAndEndedOrders = 7
        };

        // Act - Should not throw authentication exception
        var act = async () => await medicationsClient.GetCurrentMedicationsAsync(
            BaseUrl,
            request,
            tokenResponse.AccessToken);

        // Assert - Should not throw unauthorized exception
        await act.Should().NotThrowAsync<HttpRequestException>(
            "Authentication should succeed with valid bearer token");
    }

    /// <summary>
    /// Tests that GetCurrentMedications validates required parameters.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetCurrentMedicationsAsyncShouldValidateRequiredParameters()
    {
        // Arrange
        var medicationsClient = _serviceProvider.GetRequiredService<IEpicCurrentMedicationsClient>();

        var request = new CurrentMedicationsRequest
        {
            PatientId = PatientId,
            PatientIdType = "FHIR",
            ProfileView = 2,
            NumberDaysToIncludeDiscontinuedAndEndedOrders = 30
        };

        // Act & Assert - Null base URL
        var actNullUrl = async () => await medicationsClient.GetCurrentMedicationsAsync(
            null!,
            request,
            "fake-token");
        await actNullUrl.Should().ThrowAsync<ArgumentNullException>("BaseUrl is required");

        // Act & Assert - Null request
        var actNullRequest = async () => await medicationsClient.GetCurrentMedicationsAsync(
            BaseUrl,
            null!,
            "fake-token");
        await actNullRequest.Should().ThrowAsync<ArgumentNullException>("Request body is required");

        // Act & Assert - Null access token
        var actNullToken = async () => await medicationsClient.GetCurrentMedicationsAsync(
            BaseUrl,
            request,
            null!);
        await actNullToken.Should().ThrowAsync<ArgumentNullException>("Access token is required");
    }

    /// <summary>
    /// Tests that GetMedicationAdministrationHistory validates required parameters.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetMedicationAdministrationHistoryShouldValidateRequiredParameters()
    {
        // Arrange
        var medicationsClient = _serviceProvider.GetRequiredService<IEpicCurrentMedicationsClient>();

        var request = new MedicationAdministrationRequest
        {
            PatientId = PatientId,
            PatientIdType = "FHIR",
            ContactId = "12345",
            ContactIdType = "CSN",
            OrderIds = new List<IdType>()
        };

        // Act & Assert - Null base URL
        var actNullUrl = async () => await medicationsClient.GetCurrentMedicationsHistoryAsync(
            null!,
            request,
            "fake-token");
        await actNullUrl.Should().ThrowAsync<ArgumentNullException>("BaseUrl is required");

        // Act & Assert - Null request
        var actNullRequest = async () => await medicationsClient.GetCurrentMedicationsHistoryAsync(
            BaseUrl,
            null!,
            "fake-token");
        await actNullRequest.Should().ThrowAsync<ArgumentNullException>("Request body is required");

        // Act & Assert - Null access token
        var actNullToken = async () => await medicationsClient.GetCurrentMedicationsHistoryAsync(
            BaseUrl,
            request,
            null!);
        await actNullToken.Should().ThrowAsync<ArgumentNullException>("Access token is required");
    }

    /// <summary>
    /// Helper method to validate if a string is valid JSON.
    /// </summary>
    /// <param name="json">The string to validate.</param>
    /// <returns>True if the string is valid JSON, false otherwise.</returns>
    private static bool IsValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch (System.Text.Json.JsonException)
        {
            return false;
        }
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
