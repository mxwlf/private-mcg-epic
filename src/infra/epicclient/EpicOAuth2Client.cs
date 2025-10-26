using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace Mcg.Edge.Fhir.Epic.OAuthClient;

/// <summary>
/// Client for authenticating with Epic's OAuth2 service using JWT bearer tokens.
/// </summary>
public class EpicOAuth2Client : IDisposable
{
    private readonly EpicAuthClientOptions _options;
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the EpicOAuth2Client with the specified options.
    /// Creates its own HttpClient instance.
    /// </summary>
    /// <param name="options">The configuration options.</param>
    public EpicOAuth2Client(EpicAuthClientOptions options)
        : this(options, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the EpicOAuth2Client with the specified options and HttpClient.
    /// </summary>
    /// <param name="options">The configuration options.</param>
    /// <param name="httpClient">The HttpClient to use for requests. If null, a new instance will be created.</param>
    public EpicOAuth2Client(EpicAuthClientOptions options, HttpClient? httpClient)
    {
        this._options = options ?? throw new ArgumentNullException(nameof(options));

        if (httpClient == null)
        {
            this._httpClient = new HttpClient();
            this._ownsHttpClient = true;
        }
        else
        {
            this._httpClient = httpClient;
            this._ownsHttpClient = false;
        }
    }

    /// <summary>
    /// Requests an access token from Epic's OAuth2 service.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The token response containing the access token.</returns>
    /// <exception cref="HttpRequestException">Thrown when the request fails.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the response cannot be parsed.</exception>
    public async Task<TokenResponse> RequestTokenAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(this._disposed, this);

        // Generate the JWT
        var jwt = GenerateJwt();

        // Prepare the request
        var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer" },
            { "client_assertion", jwt }
        });

        requestContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded")
        {
            CharSet = "utf-8"
        };

        // Send the request
        var response = await this._httpClient.PostAsync(this._options.TokenEndpoint, requestContent, cancellationToken);

        // Read response content
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        // Check for errors and include response content in exception
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Token request failed with status {(int)response.StatusCode} ({response.StatusCode}). Response: {responseContent}");
        }

        // Parse the response
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);

        if (tokenResponse == null)
        {
            throw new InvalidOperationException("Failed to deserialize token response.");
        }

        return tokenResponse;
    }

    /// <summary>
    /// Generates a JWT signed with RS384 for Epic OAuth2 authentication.
    /// </summary>
    /// <returns>The signed JWT string.</returns>
    private string GenerateJwt()
    {
        var now = DateTimeOffset.UtcNow;
        var exp = now.AddSeconds(this._options.JwtExpirationSeconds);

        // Load the private key
        var rsa = LoadRsaPrivateKey(this._options.PrivateKeyPem);
        var signingCredentials = new SigningCredentials(
            new RsaSecurityKey(rsa),
            SecurityAlgorithms.RsaSha384
        );

        // Create the JWT using SecurityTokenDescriptor
        // Set time properties directly on descriptor for proper numeric formatting
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = this._options.ClientId,
            Audience = this._options.TokenEndpoint,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = exp.UtcDateTime,
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, this._options.ClientId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }),
            SigningCredentials = signingCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Loads an RSA private key from PEM format.
    /// </summary>
    /// <param name="privateKeyPem">The private key in PEM format.</param>
    /// <returns>An RSA instance with the loaded key.</returns>
    private static RSA LoadRsaPrivateKey(string privateKeyPem)
    {
        var rsa = RSA.Create();

        // Remove PEM header/footer and whitespace
        var keyText = privateKeyPem
            .Replace("-----BEGIN PRIVATE KEY-----", "")
            .Replace("-----END PRIVATE KEY-----", "")
            .Replace("\r", "")
            .Replace("\n", "")
            .Trim();

        // Decode from base64
        var keyBytes = Convert.FromBase64String(keyText);

        // Import the key (PKCS#8 format)
        rsa.ImportPkcs8PrivateKey(keyBytes, out _);

        return rsa;
    }

    /// <summary>
    /// Disposes the client and its resources.
    /// </summary>
    public void Dispose()
    {
        if (!this._disposed)
        {
            if (this._ownsHttpClient)
            {
                this._httpClient.Dispose();
            }
            this._disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}

public class TokenResponse
{
    public string AccessToken { get; set; }
    public string TokeType { get; set; }
    public int ExpiresIn { get; set; }
    public string Scope { get; set; }
}