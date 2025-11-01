// ******************
// © 2025 MCG Health.
// ******************

namespace Mcg.Edge.Fhir.Epic.Abstractions;

/// <summary>
/// Constants for Epic FHIR custom API implementations.
/// </summary>
public static class EpicConstants
{
    /// <summary>
    /// Default contact ID type for Epic encounters (CSN = Contact Serial Number).
    /// </summary>
    public const string DefaultContactType = "CSN";

    /// <summary>
    /// Patient ID type for Epic API calls (always "FHIR" for FHIR-based identifiers).
    /// </summary>
    public const string PatientIdType = "FHIR";

    /// <summary>
    /// Profile view parameter for Epic GetCurrentMedications API.
    /// Value 2 provides a specific medication view format.
    /// </summary>
    public const int ProfileView = 2;

    /// <summary>
    /// Order ID type for internal Epic order identifiers.
    /// </summary>
    public const string InternalOrderIdType = "Internal";
}
