// ******************
// © 2025 MCG Health.
// ******************

using System.Text.Json.Serialization;

namespace Mcg.Edge.Fhir.Epic.Abstractions;

/// <summary>
/// Represents a medication order with its administration history.
/// </summary>
public class MedicationAdminOrder
{
    /// <summary>
    /// Gets or sets the order identifier.
    /// </summary>
    [JsonPropertyName("orderID")]
    public IdType? OrderId { get; set; }

    /// <summary>
    /// Gets or sets the name of the medication.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the medication is currently active.
    /// </summary>
    [JsonPropertyName("isActive")]
    public bool? IsActive { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the medication is an infusion.
    /// </summary>
    [JsonPropertyName("isInfusion")]
    public bool? IsInfusion { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the medication is a mixture.
    /// </summary>
    [JsonPropertyName("isMixture")]
    public bool? IsMixture { get; set; }

    /// <summary>
    /// Gets or sets the linked order IDs if this medication is part of a linked order group.
    /// </summary>
    [JsonPropertyName("linkedOrderIDs")]
    public List<IdType>? LinkedOrderIds { get; set; }

    /// <summary>
    /// Gets or sets the linked order type (can be "And", "Or", or "Followed By").
    /// </summary>
    [JsonPropertyName("linkedOrderType")]
    public string? LinkedOrderType { get; set; }

    /// <summary>
    /// Gets or sets the array of medication administrations associated with this order.
    /// </summary>
    [JsonPropertyName("medicationAdministrations")]
    public List<MedicationAdministration>? MedicationAdministrations { get; set; }
}
