// ******************
// © 2025 MCG Health.
// ******************

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Mcg.Edge.Fhir.Epic.Abstractions;

/// <summary>
/// Request model for Epic's GetMedicationAdministrationHistory custom API.
/// Retrieves medication administration history for specific orders during an encounter.
/// </summary>
public class MedicationAdministrationRequest
{
    /// <summary>
    /// Gets or sets the patient identifier.
    /// </summary>
    [Required]
    [JsonPropertyName("patientID")]
    public required string PatientId { get; set; }

    /// <summary>
    /// Gets or sets the patient ID type. Should always be "FHIR" for FHIR-based identifiers.
    /// </summary>
    [Required]
    [JsonPropertyName("patientIDType")]
    public required string PatientIdType { get; set; }

    /// <summary>
    /// Gets or sets the encounter/contact identifier.
    /// </summary>
    [Required]
    [JsonPropertyName("contactID")]
    public required string ContactId { get; set; }

    /// <summary>
    /// Gets or sets the contact ID type (typically "CSN" for Contact Serial Number).
    /// </summary>
    [Required]
    [JsonPropertyName("contactIDType")]
    public required string ContactIdType { get; set; }

    /// <summary>
    /// Gets or sets the array of order IDs to retrieve administration history for.
    /// </summary>
    [Required]
    [JsonPropertyName("orderIDs")]
    public required List<IdType> OrderIds { get; set; }
}
