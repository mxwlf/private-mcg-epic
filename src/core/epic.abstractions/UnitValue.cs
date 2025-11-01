// ******************
// © 2025 MCG Health.
// ******************

using System.Text.Json.Serialization;

namespace Mcg.Edge.Fhir.Epic.Abstractions;

/// <summary>
/// Represents a value with its associated unit of measurement.
/// Used for doses, rates, and other measured quantities in medication data.
/// </summary>
public class UnitValue
{
    /// <summary>
    /// Gets or sets the numeric value as a string.
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the unit of measurement (e.g., "mg", "mL", "units").
    /// </summary>
    [JsonPropertyName("unit")]
    public string? Unit { get; set; }
}
