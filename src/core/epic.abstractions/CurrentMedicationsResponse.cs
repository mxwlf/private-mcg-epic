// ******************
// © 2025 MCG Health.
// ******************

using System.Text.Json.Serialization;

namespace Mcg.Edge.Fhir.Epic.Abstractions;

/// <summary>
/// Response model for Epic's GetCurrentMedications API.
/// Contains the patient's current medication orders.
/// </summary>
public class CurrentMedicationsResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether there was a problem loading orders.
    /// </summary>
    [JsonPropertyName("hasProblemLoadingOrders")]
    public bool? HasProblemLoadingOrders { get; set; }

    /// <summary>
    /// Gets or sets information about any problems loading orders.
    /// </summary>
    [JsonPropertyName("problemLoadingOrdersInformation")]
    public string? ProblemLoadingOrdersInformation { get; set; }

    /// <summary>
    /// Gets or sets the start date for including discontinued and ended orders.
    /// </summary>
    [JsonPropertyName("includeDiscontinuedAndEndedOrdersFromDate")]
    public string? IncludeDiscontinuedAndEndedOrdersFromDate { get; set; }

    /// <summary>
    /// Gets or sets the end date for including discontinued and ended orders.
    /// </summary>
    [JsonPropertyName("includeDiscontinuedAndEndedOrdersToDate")]
    public string? IncludeDiscontinuedAndEndedOrdersToDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the patient is currently admitted.
    /// </summary>
    [JsonPropertyName("isPatientAdmitted")]
    public bool? IsPatientAdmitted { get; set; }

    /// <summary>
    /// Gets or sets the collection of medication orders.
    /// </summary>
    [JsonPropertyName("medicationOrders")]
    public List<MedicationOrder>? MedicationOrders { get; set; }
}
