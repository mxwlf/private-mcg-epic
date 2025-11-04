// ******************
// © 2025 MCG Health.
// ******************

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Mcg.Edge.Fhir.Epic.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Mcg.Edge.Fhir.Epic.HttpClients;

/// <summary>
/// HTTP client for Epic's custom medication APIs.
/// Implements Epic-specific medication endpoints for GetCurrentMedications and GetMedicationAdministrationHistory.
/// </summary>
public class EpicMedicationsHttpClient : IEpicCurrentMedicationsClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EpicMedicationsHttpClient> _logger;
    private readonly IOptionsMonitor<EpicMedicationsClientOptions> _optionsMonitor;

    /// <summary>
    /// Initializes a new instance of the <see cref="EpicMedicationsHttpClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client instance (injected via IHttpClientFactory).</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="optionsMonitor">The configuration options monitor.</param>
    public EpicMedicationsHttpClient(
        HttpClient httpClient,
        ILogger<EpicMedicationsHttpClient> logger,
        IOptionsMonitor<EpicMedicationsClientOptions> optionsMonitor)
    {
        this._httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this._optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
    }

    /// <inheritdoc/>
    public async Task<string> GetCurrentMedicationsAsync(
        string baseUrl,
        CurrentMedicationsRequest body,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(baseUrl);
        ArgumentNullException.ThrowIfNull(body);
        ArgumentNullException.ThrowIfNull(accessToken);

        var options = this._optionsMonitor.CurrentValue;
        var url = CombineUrl(baseUrl, options.GetCurrentMedicationsPath);

        this._logger.LogInformation(
            "Calling Epic GetCurrentMedications API for patient {PatientId}",
            body.PatientId);

        return await PostJsonAsync(url, body, accessToken, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<string> GetCurrentMedicationsHistoryAsync(
        string baseUrl,
        MedicationAdministrationRequest body,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(baseUrl);
        ArgumentNullException.ThrowIfNull(body);
        ArgumentNullException.ThrowIfNull(accessToken);

        var options = this._optionsMonitor.CurrentValue;
        var url = CombineUrl(baseUrl, options.GetMedicationAdministrationHistoryPath);

        this._logger.LogInformation(
            "Calling Epic GetMedicationAdministrationHistory API for patient {PatientId}, contact {ContactId}, with {OrderCount} orders",
            body.PatientId,
            body.ContactId,
            body.OrderIds?.Count ?? 0);

        return await PostJsonAsync(url, body, accessToken, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Posts a JSON request to the specified URL and returns the response as a JSON string.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request body.</typeparam>
    /// <param name="url">The complete URL to post to.</param>
    /// <param name="body">The request body to serialize.</param>
    /// <param name="accessToken">The OAuth2 Bearer token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response body as a JSON string.</returns>
    /// <exception cref="HttpRequestException">Thrown when the request fails.</exception>
    private async Task<string> PostJsonAsync<TRequest>(
        Uri url,
        TRequest body,
        string accessToken,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url);

        // Set authentication header
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Set content headers
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/fhir+json"));

        // Serialize the request body
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null, // Use the JsonPropertyName attributes as-is
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        request.Content = JsonContent.Create(body, options: jsonOptions);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json")
        {
            CharSet = "utf-8"
        };

        // Send the request
        this._logger.LogDebug("Sending POST request to {Url}", url);

        var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        // Read the response content
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        // Check for errors
        if (!response.IsSuccessStatusCode)
        {
            this._logger.LogError(
                "Epic API request failed with status {StatusCode}: {ResponseBody}",
                (int)response.StatusCode,
                responseContent);

            throw new HttpRequestException(
                $"Epic API request failed with status {(int)response.StatusCode} ({response.StatusCode}). " +
                $"Response: {responseContent}");
        }

        this._logger.LogDebug("Received successful response from Epic API");

        return responseContent;
    }

    /// <summary>
    /// Combines a base URL with a path, handling trailing and leading slashes.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <param name="path">The path to append.</param>
    /// <returns>The combined URL as a Uri object.</returns>
    private static Uri CombineUrl(string baseUrl, string path)
    {
        var trimmedBase = baseUrl.TrimEnd('/');
        var trimmedPath = path.TrimStart('/');
        var combinedUrl = $"{trimmedBase}/{trimmedPath}";
        return new Uri(combinedUrl, UriKind.Absolute);
    }
}
