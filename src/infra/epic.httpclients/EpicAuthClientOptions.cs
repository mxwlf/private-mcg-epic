using System.ComponentModel.DataAnnotations;

namespace Mcg.Edge.Fhir.Epic.HttpClients;

/// <summary>
/// Configuration options for the Epic OAuth2 client.
/// </summary>
public class EpicAuthClientOptions
{
    /// <summary>
    /// The client ID used as both 'iss' and 'sub' in the JWT.
    /// </summary>
    [Required(ErrorMessage = "ClientId is required")]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// The token endpoint URL (used as 'aud' in the JWT).
    /// </summary>
    [Required(ErrorMessage = "TokenEndpoint is required")]
    [Url(ErrorMessage = "TokenEndpoint must be a valid URL")]
    public string TokenEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// The private key in PEM format (including BEGIN/END markers) for RS384 signing.
    /// </summary>
    [Required(ErrorMessage = "PrivateKey is required")]
    public string PrivateKey { get; set; } = string.Empty;

    /// <summary>
    /// JWT expiration time in seconds. Default is 240 seconds (4 minutes).
    /// </summary>
    [Range(1, 3600, ErrorMessage = "JwtExpirationSeconds must be between 1 and 3600")]
    public int JwtExpirationSeconds { get; set; } = 240;
}
