// ******************
// © 2025 MCG Health.
// ******************

using System.Text.Json.Serialization;

namespace Mcg.Edge.Fhir.Epic.Abstractions;

/// <summary>
/// Response model for Epic's GetMedicationAdministrationHistory API.
/// Contains medication orders with their administration history.
/// </summary>
public class MedicationAdministrationResponse
{
    /// <summary>
    /// Gets or sets the collection of medication orders with their administration events.
    /// </summary>
    [JsonPropertyName("Orders")]
    public List<MedicationAdminOrder>? Orders { get; set; }
}
