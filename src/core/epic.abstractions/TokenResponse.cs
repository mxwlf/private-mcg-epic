namespace Mcg.Edge.Fhir.Epic.Abstractions;

public class TokenResponse
{
    public string AccessToken { get; set; }
    public string TokeType { get; set; }
    public int ExpiresIn { get; set; }
    public string Scope { get; set; }
}
