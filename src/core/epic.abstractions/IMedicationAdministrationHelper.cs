// ******************
// © 2025 MCG Health.
// ******************

using Hl7.Fhir.Model;

namespace Mcg.Edge.Fhir.Epic.Abstractions;

/// <summary>
/// Interface for Medication Request Helper.
/// </summary>
public interface IMedicationAdministrationHelper
{
    /// <summary>
    /// Builds the medication request bundle from the given transformation result.
    /// </summary>
    /// <param name="resultToSet">The transformation result containing data to build the bundle.</param>
    /// <returns>A JSON string representing the medication request bundle.</returns>
    string BuildMedicationRequestBundle(TransformationResult resultToSet);

    /// <summary>
    /// Gets the medication codeable concept from a medication order.
    /// </summary>
    /// <param name="medicationOrder">The medication order to extract the codeable concept from.</param>
    /// <returns>A <see cref="CodeableConcept"/> representing the medication.</returns>
    CodeableConcept GetMedicationCodeableConcept(MedicationOrder medicationOrder);
}
