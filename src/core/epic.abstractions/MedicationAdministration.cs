// ******************
// © 2025 MCG Health.
// ******************

using System.Text.Json.Serialization;

namespace Mcg.Edge.Fhir.Epic.Abstractions;

/// <summary>
/// Represents a single medication administration event.
/// </summary>
public class MedicationAdministration
{
    /// <summary>
    /// Gets or sets the action associated with this administration (e.g., "Given", "Held", "Refused").
    /// </summary>
    [JsonPropertyName("action")]
    public string? Action { get; set; }

    /// <summary>
    /// Gets or sets the date and time of this administration action.
    /// </summary>
    [JsonPropertyName("administrationInstant")]
    public DateTimeOffset? AdministrationInstant { get; set; }

    /// <summary>
    /// Gets or sets the dose administered.
    /// </summary>
    [JsonPropertyName("dose")]
    public UnitValue? Dose { get; set; }

    /// <summary>
    /// Gets or sets the rate of administration for infusions.
    /// </summary>
    [JsonPropertyName("rate")]
    public UnitValue? Rate { get; set; }

    /// <summary>
    /// Gets or sets the mapped action if this administration's action is a custom action mapped to a standard action.
    /// </summary>
    [JsonPropertyName("mappedAction")]
    public string? MappedAction { get; set; }

    /// <summary>
    /// Gets or sets the array containing the order ID (and type) for the linked override order.
    /// </summary>
    [JsonPropertyName("linkedOverrideOrderID")]
    public List<IdType>? LinkedOverrideOrderId { get; set; }
}
