// ******************
// © 2025 MCG Health.
// ******************

using System.Text.Json.Serialization;

namespace Mcg.Edge.Fhir.Epic.Abstractions;

/// <summary>
/// Represents an identifier with its type in Epic's system.
/// Used for order IDs, contact IDs, and other identifiers.
/// </summary>
public class IdType
{
    /// <summary>
    /// Gets or sets the identifier value.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the type of identifier (e.g., "Internal", "CSN", "FHIR").
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
