// <copyright file="FhirClient.cs" company="Hearst Company">
// Copyright (c) Hearst Company. All rights reserved.
// </copyright>

namespace Fhir.Plugin.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using Fhir.Plugin.Abstractions;
    using FhirService.Plugin.Models;
    using Mcg.Edge.Common.Configuration;
    using Mcg.Edge.Domain.Clients;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// Client for interacting with FHIR server to read and manage FHIR resources.
    /// </summary>
    public class FhirClient : IFhirClient
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger logger;
        private readonly IPHILogger phiLogger;

        /// <summary>
        /// Gets the activity source for tracing purposes.
        /// </summary>
        public ActivitySource ActivitySource { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FhirClient"/> class.
        /// </summary>
        /// <param name="httpClientFactory">The HTTP client factory used to create HTTP clients.</param>
        /// <param name="logger">The logger for non-PHI logging information.</param>
        /// <param name="phiLogger">The logger for PHI-related information.</param>
        public FhirClient(IHttpClientFactory httpClientFactory, ILogger logger, IPHILogger phiLogger)
        {
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
            this.phiLogger = phiLogger;
        }

        /// <summary>
        /// Gets the FHIR resource from the server.
        /// </summary>
        /// <param name="queryString">The complete URL query string for the FHIR resource.</param>
        /// <param name="accessToken">The OAuth2 access token for authentication.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A string containing the FHIR resource in JSON format.</returns>
        /// <exception cref="ApiException">Thrown when the API returns an unexpected response.</exception>
        public async Task<string> GetFhirResourceAsync(string queryString, string accessToken, CancellationToken cancellationToken = default)
        {
            return await this.SendRequestAsync(HttpMethod.Get, queryString, accessToken, cancellationToken);
        }

        /// <summary>
        /// Gets the Medication requests from Epic custom server.
        /// </summary>
        /// <param name="baseUrl">Base URL of the custom Epic server.</param>
        /// <param name="body">The Epic current medications request data.</param>
        /// <param name="accessToken">The OAuth2 access token for authentication.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A JSON string containing the medication data.</returns>
        /// <exception cref="ApiException">Thrown when the API returns an unexpected response.</exception>
        public async Task<string> GetCurrentMedicationsAsync(string baseUrl, EpicCurrentMedicationsRequest body, string accessToken, CancellationToken cancellationToken = default)
        {
            var json = JsonConvert.SerializeObject(body);
            this.phiLogger.LogInformation($"FHIR client Sending message to GetCurrentMedications call: {json}");

            var content = new StringContent(json);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            return await this.SendRequestAsync(HttpMethod.Post, baseUrl, accessToken, cancellationToken, content);
        }

        /// <summary>
        /// Gets the medication administration history from Epic custom server.
        /// </summary>
        /// <param name="baseUrl">Base URL of the custom Epic server.</param>
        /// <param name="body">The Epic current medication administration request data.</param>
        /// <param name="accessToken">The OAuth2 access token for authentication.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A JSON string containing the medication administration history data.</returns>
        /// <exception cref="ApiException">Thrown when the API returns an unexpected response.</exception>
        public async Task<string> GetCurrentMedicationsHistoryAsync(string baseUrl, EpicMedicationAdministrationRequest body, string accessToken, CancellationToken cancellationToken = default)
        {
            var json = JsonConvert.SerializeObject(body);
            this.phiLogger.LogInformation($"FHIR client Sending message to GetCurrentMedicationsHistory call: {json}");

            var content = new StringContent(json);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            return await this.SendRequestAsync(HttpMethod.Post, baseUrl, accessToken, cancellationToken, content);
        }

        /// <summary>
        /// Prepares the HTTP request by adding the authorization header.
        /// </summary>
        /// <param name="client">The HTTP client.</param>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="url">The request URL.</param>
        /// <param name="accessToken">The OAuth2 access token.</param>
        public void PrepareRequest(HttpClient client, HttpRequestMessage request, string url, string accessToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        /// <summary>
        /// Starts an activity for tracing purposes.
        /// </summary>
        /// <param name="endpoint">The endpoint name for the activity.</param>
        /// <returns>The started activity or null if ActivitySource is not configured.</returns>
        protected virtual Activity StartActivity(string endpoint)
        {
            Activity activity = null;

            if (this.ActivitySource != null)
            {
                activity = this.ActivitySource.StartActivity(endpoint);
            }

            return activity;
        }

        /// <summary>
        /// Ends the activity for tracing.
        /// </summary>
        /// <param name="activity">The activity to end.</param>
        protected virtual void EndActivity(Activity activity)
        {
            activity?.Stop();
        }

        /// <summary>
        /// Sends an HTTP request to the FHIR server.
        /// </summary>
        /// <param name="method">The HTTP method to use.</param>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="accessToken">The OAuth2 access token for authentication.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <param name="content">Optional HTTP content to include in the request.</param>
        /// <returns>A string containing the response from the server.</returns>
        /// <exception cref="ApiException">Thrown when the API returns an unexpected response.</exception>
        private async Task<string> SendRequestAsync(HttpMethod method, string url, string accessToken, CancellationToken cancellationToken, HttpContent content = null)
        {
            HttpClient client = this.httpClientFactory.CreateClient();
            var disposeClient = false;
            Activity startedActivity = null;

            try
            {
                startedActivity = this.StartActivity(method.Method);

                using (var request = new HttpRequestMessage(method, new Uri(url, UriKind.RelativeOrAbsolute)))
                {
                    request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/fhir+json"));
                    request.Content = content;

                    this.PrepareRequest(client, request, url, accessToken);
                    this.phiLogger?.LogInformation($"FHIR client Request {request}");
                    var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    return await this.HandleResponseAsync(response);
                }
            }
            finally
            {
                if (disposeClient)
                {
                    client.Dispose();
                }

                this.EndActivity(startedActivity);
            }
        }

        /// <summary>
        /// Handles the HTTP response from the FHIR server.
        /// </summary>
        /// <param name="response">The HTTP response message.</param>
        /// <returns>A string containing the response content.</returns>
        /// <exception cref="ApiException">Thrown when the response status code is not OK or the response is empty.</exception>
        private async Task<string> HandleResponseAsync(HttpResponseMessage response)
        {
            var headers = new Dictionary<string, IEnumerable<string>>();

            foreach (var item in response.Headers)
            {
                headers[item.Key] = item.Value;
            }

            if (response.Content != null && response.Content.Headers != null)
            {
                foreach (var item in response.Content.Headers)
                {
                    headers[item.Key] = item.Value;
                }
            }

            var status = (int)response.StatusCode;
            var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            this.phiLogger?.LogInformation($"FHIR client Status Code: {status} Response: {responseText}.");

            return response.StatusCode switch
            {
                HttpStatusCode.OK => string.IsNullOrEmpty(responseText)
                    ? throw new ApiException("Response was null which was not expected.", status, responseText, headers, null)
                    : responseText,
                HttpStatusCode.BadRequest => throw new ApiException("Bad Request", status, responseText, headers, null),
                HttpStatusCode.Unauthorized => throw new ApiException("Unauthorized", status, responseText, headers, null),
                HttpStatusCode.Forbidden => throw new ApiException("Forbidden", status, responseText, headers, null),
                HttpStatusCode.NotFound => throw new ApiException("Not Found", status, responseText, headers, null),
                HttpStatusCode.InternalServerError => throw new ApiException("Internal server error", status, responseText, headers, null),
                HttpStatusCode.ServiceUnavailable => throw new ApiException("Service unavailable", status, responseText, headers, null),
                _ => throw new ApiException($"The HTTP status code of the response was not expected ({status}).", status, responseText, headers, null)
            };
        }
    }
}
