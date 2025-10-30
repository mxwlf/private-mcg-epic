namespace Mcg.Edge.Fhir.Epic.HttpClients;

/// <summary>
/// Configuration options for the Epic OAuth2 client.
/// </summary>
public class EpicAuthClientOptions
{
    /// <summary>
    /// The client ID used as both 'iss' and 'sub' in the JWT.
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// The token endpoint URL (used as 'aud' in the JWT).
    /// </summary>
    public required string TokenEndpoint { get; init; }

    /// <summary>
    /// The private key in PEM format (including BEGIN/END markers) for RS384 signing.
    /// </summary>
    public required string PrivateKeyPem { get; init; }

    /// <summary>
    /// JWT expiration time in seconds. Default is 240 seconds (4 minutes).
    /// </summary>
    public int JwtExpirationSeconds { get; init; } = 240;
}
