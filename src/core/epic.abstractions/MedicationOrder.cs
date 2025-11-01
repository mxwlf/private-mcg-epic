// ******************
// © 2025 MCG Health.
// ******************

using System.Text.Json.Serialization;

namespace Mcg.Edge.Fhir.Epic.Abstractions;

/// <summary>
/// Represents a medication order from Epic's GetCurrentMedications API.
/// This is a simplified model focusing on the core properties needed for medication workflows.
/// Additional properties from Epic can be added as needed.
/// </summary>
public class MedicationOrder
{
    /// <summary>
    /// Gets or sets the collection of identifiers for this medication order.
    /// </summary>
    [JsonPropertyName("ids")]
    public List<IdType>? Ids { get; set; }

    /// <summary>
    /// Gets or sets the name of the medication.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the start date/time of the medication order.
    /// </summary>
    [JsonPropertyName("startDateTime")]
    public DateTimeOffset? StartDateTime { get; set; }

    /// <summary>
    /// Gets or sets the end date/time of the medication order.
    /// </summary>
    [JsonPropertyName("endDateTime")]
    public DateTimeOffset? EndDateTime { get; set; }

    /// <summary>
    /// Gets or sets the discontinue date/time.
    /// </summary>
    [JsonPropertyName("discontinueInstant")]
    public DateTimeOffset? DiscontinueInstant { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the medication is long term.
    /// </summary>
    [JsonPropertyName("isLongTerm")]
    public bool? IsLongTerm { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the medication is a mixture.
    /// </summary>
    [JsonPropertyName("isMixture")]
    public bool? IsMixture { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the medication is suspended.
    /// </summary>
    [JsonPropertyName("isSuspended")]
    public bool? IsSuspended { get; set; }

    /// <summary>
    /// Gets or sets the dose information.
    /// </summary>
    [JsonPropertyName("dose")]
    public string? Dose { get; set; }

    /// <summary>
    /// Gets or sets the ordered dose.
    /// </summary>
    [JsonPropertyName("orderedDose")]
    public string? OrderedDose { get; set; }

    /// <summary>
    /// Gets or sets the order mode (e.g., "Inpatient", "Outpatient").
    /// </summary>
    [JsonPropertyName("orderMode")]
    public string? OrderMode { get; set; }
}
