// ******************
// © 2025 MCG Health.
// ******************

using System.ComponentModel.DataAnnotations;

namespace Mcg.Edge.Fhir.Epic.HttpClients;

/// <summary>
/// Configuration options for the Epic Medications HTTP client.
/// </summary>
public class EpicMedicationsClientOptions
{
    /// <summary>
    /// Gets or sets the base URL for Epic's custom medication APIs.
    /// Example: "https://fhir.epic.com/interconnect-fhir-oauth"
    /// </summary>
    // [Required]
    // public required string BaseUrl { get; set; }

    public string ClientName { get; set; } = "EpicClient";

    /// <summary>
    /// Gets or sets the relative path for the GetCurrentMedications API endpoint.
    /// Default: "/api/epic/2014/Clinical/Patient/GETMEDICATIONSV2/GetCurrentMedications"
    /// </summary>
    public required string GetCurrentMedicationsPath { get; set; } = "/api/epic/2014/Clinical/Patient/GETMEDICATIONSV2/GetCurrentMedications";

    /// <summary>
    /// Gets or sets the relative path for the GetMedicationAdministrationHistory API endpoint.
    /// Default: "/api/epic/2014/Clinical/Patient/MEDICATIONADMINISTRATION/GetMedicationAdministrationHistory"
    /// </summary>
    public required string GetMedicationAdministrationHistoryPath { get; set; } = "/api/epic/2014/Clinical/Patient/MEDICATIONADMINISTRATION/GetMedicationAdministrationHistory";

    /// <summary>
    /// Gets or sets the request timeout in seconds. Default is 30 seconds.
    /// </summary>
    [Range(1, 300)]
    public int TimeoutSeconds { get; set; } = 30;
}
