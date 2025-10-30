// <copyright file="IFhirClient.cs" company="Hearst Company">
// Copyright (c) Hearst Company. All rights reserved.
// </copyright>

namespace Fhir.Plugin.Abstractions
{
    using System.Threading;
    using System.Threading.Tasks;
    using FhirService.Plugin.Models;

    /// <summary>
    /// Interface for interacting with FHIR resources.
    /// </summary>
    public interface IFhirClient
    {
        /// <summary>
        /// Gets a FHIR resource from the server.
        /// </summary>
        /// <param name="queryString">The complete URL query string for the FHIR resource.</param>
        /// <param name="accessToken">The OAuth2 access token for authentication.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A string containing the FHIR resource in JSON format.</returns>
        Task<string> GetFhirResourceAsync(string queryString, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the Medication requests from epic custom server.
        /// </summary>
        /// <param name="baseUrl">Base url of the custom epic server.</param>
        /// <param name="body">EpicCurrentMedicationsRequest.</param>
        /// <param name="accessToken">The OAuth2 access token for authentication.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<string> GetCurrentMedicationsAsync(string baseUrl, EpicCurrentMedicationsRequest body, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the Medication History from epic custom server.
        /// </summary>
        /// <param name="baseUrl">Base url of the custom epic server.</param>
        /// <param name="body">The epic custom request.</param>
        /// <param name="accessToken">The OAuth2 access token for authentication.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<string> GetCurrentMedicationsHistoryAsync(string baseUrl, EpicMedicationAdministrationRequest body, string accessToken, CancellationToken cancellationToken = default);
    }
}
