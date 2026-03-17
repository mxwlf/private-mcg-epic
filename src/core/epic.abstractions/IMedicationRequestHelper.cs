// ******************
// © 2025 MCG Health.
// ******************

using Hl7.Fhir.Model;

namespace Mcg.Edge.Fhir.Epic.Abstractions;

/// <summary>
/// Provides helper methods for handling FHIR MedicationRequest resources.
/// </summary>
public interface IMedicationRequestHelper
{
    /// <summary>
    /// Builds a FHIR Bundle containing MedicationRequest resources.
    /// </summary>
    /// <param name="resultToSet">The transformation result to process.</param>
    /// <returns>A JSON string representation of the FHIR Bundle.</returns>
    string BuildMedicationRequestBundle(TransformationResult resultToSet);

    /// <summary>
    /// Retrieves information about a MedicationRequest resource.
    /// </summary>
    /// <param name="order">The medication order.</param>
    /// <param name="medicationOrderId">The unique identifier for the medication order.</param>
    /// <param name="medicationRequests">JSON string containing medication requests.</param>
    /// <returns>A MedicationRequestInfo object containing reference and status information.</returns>
    MedicationRequestInfo GetMedicationRequestInfo(MedicationOrder order, string medicationOrderId,
        string medicationRequests);

    /// <summary>
    /// Creates a CodeableConcept for a medication based on the provided order.
    /// </summary>
    /// <param name="medicationOrder">The medication order containing medication details.</param>
    /// <returns>A CodeableConcept representing the medication.</returns>
    CodeableConcept GetMedicationCodeableConcept(MedicationOrder medicationOrder);

    /// <summary>
    /// Creates a list of identifiers for a medication request.
    /// </summary>
    /// <param name="orderId">The unique identifier for the order.</param>
    /// <returns>A list of FHIR Identifier objects.</returns>
    List<Identifier> GetMedicationRequestIdentifiers(string orderId);

    /// <summary>
    /// Determines the appropriate category for a medication request.
    /// </summary>
    /// <param name="order">The medication order to categorize.</param>
    /// <returns>A list of CodeableConcept objects representing the medication categories.</returns>
    List<CodeableConcept> GetMedicationRequestCategory(MedicationOrder order);

    /// <summary>
    /// Extracts dosage instructions from a medication order.
    /// </summary>
    /// <typeparam name="T">The type of dosage instruction (typically Dosage in R4).</typeparam>
    /// <param name="medicationOrder">The medication order containing dosage details.</param>
    /// <returns>A list of dosage instruction objects of type T.</returns>
    List<T> GetDosageInstruction<T>(MedicationOrder medicationOrder);
}

/// <summary>
/// Contains reference and status information for a MedicationRequest.
/// </summary>
public class MedicationRequestInfo
{
    /// <summary>
    /// Gets or sets the reference to the MedicationRequest resource.
    /// </summary>
    public ResourceReference MedicationRequestReference { get; set; }

    /// <summary>
    /// Gets or sets the status of the MedicationRequest.
    /// </summary>
    // public R4_Fhir.MedicationRequest.MedicationrequestStatus MedicationRequestStatus { get; set; }
    public MedicationRequestStatus MedicationRequestStatus { get; set; }

}
