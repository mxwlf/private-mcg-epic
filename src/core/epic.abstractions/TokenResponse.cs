// ******************
// © 2025 MCG Health.
// ******************

#region

using System.Text.Json.Serialization;

#endregion

namespace Mcg.Edge.Fhir.Epic.Abstractions;

public class TokenResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; }

    [JsonPropertyName("token_type")] public string TokenType { get; set; }

    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }

    [JsonPropertyName("scope")] public string Scope { get; set; }
}
