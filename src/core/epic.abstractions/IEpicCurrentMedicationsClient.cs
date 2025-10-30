// ******************
// © 2025 MCG Health.
// ******************

namespace Mcg.Edge.Fhir.Epic.Abstractions;

public interface IEpicCurrentMedicationsClient
{

    /// <summary>
    /// Gets the Medication requests from epic custom server.
    /// </summary>
    /// <param name="baseUrl">Base url of the custom epic server.</param>
    /// <param name="body">EpicCurrentMedicationsRequest.</param>
    /// <param name="accessToken">The OAuth2 access token for authentication.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<string> GetCurrentMedicationsAsync(string baseUrl, CurrentMedicationsRequest body, string accessToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the Medication History from epic custom server.
    /// </summary>
    /// <param name="baseUrl">Base url of the custom epic server.</param>
    /// <param name="body">The epic custom request.</param>
    /// <param name="accessToken">The OAuth2 access token for authentication.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<string> GetCurrentMedicationsHistoryAsync(string baseUrl, MedicationAdministrationRequest body, string accessToken, CancellationToken cancellationToken = default);
}
