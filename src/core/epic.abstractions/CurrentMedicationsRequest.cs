// ******************
// © 2025 MCG Health.
// ******************

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Mcg.Edge.Fhir.Epic.Abstractions;

/// <summary>
/// Request model for Epic's GetCurrentMedications custom API.
/// Retrieves current medication orders for a patient.
/// </summary>
public class CurrentMedicationsRequest
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
    /// Gets or sets the optional user identifier.
    /// </summary>
    [JsonPropertyName("userID")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UserId { get; set; }

    /// <summary>
    /// Gets or sets the optional user ID type.
    /// </summary>
    [JsonPropertyName("userIDType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UserIdType { get; set; }

    /// <summary>
    /// Gets or sets the profile view parameter. Should be 2 for standard medication view.
    /// </summary>
    [Required]
    [JsonPropertyName("profileView")]
    public required int ProfileView { get; set; }

    /// <summary>
    /// Gets or sets the number of days to include discontinued and ended orders in the lookback period.
    /// </summary>
    [Required]
    [JsonPropertyName("numberDaysToIncludeDiscontinuedAndEndedOrders")]
    public required int NumberDaysToIncludeDiscontinuedAndEndedOrders { get; set; }
}
