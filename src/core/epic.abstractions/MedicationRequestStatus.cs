// ******************
// © 2025 MCG Health.
// ******************

namespace Mcg.Edge.Fhir.Epic.Abstractions;

public enum MedicationRequestStatus
{
    // [EnumLiteral("active"), Description("Active")]
    /// <summary>
    /// The prescription is 'actionable', but not all actions that are implied by it have occurred yet. (system: http://hl7.org/fhir/CodeSystem/medicationrequest-status)
    /// </summary>
    Active,

    /// <summary>
    /// Actions implied by the prescription are to be temporarily halted, but are expected to continue later.  May also be called 'suspended'.
    /// (system: http://hl7.org/fhir/CodeSystem/medicationrequest-status)
    /// </summary>
    // [EnumLiteral("on-hold"), Description("On Hold")]
    OnHold,

    /// <summary>
    /// The prescription has been withdrawn before any administrations have occurred
    /// (system: http://hl7.org/fhir/CodeSystem/medicationrequest-status)
    /// </summary>
    // [EnumLiteral("cancelled"), Description("Cancelled")]
    Cancelled,

    /// <summary>
    /// All actions that are implied by the prescription have occurred.
    /// (system: http://hl7.org/fhir/CodeSystem/medicationrequest-status)
    /// </summary>
    // [EnumLiteral("completed"), Description("Completed")]
    Completed,

    /// <summary>
    /// Some of the actions that are implied by the medication request may have occurred.  For example, the medication may have been dispensed and the patient may have taken some of the medication.  Clinical decision support systems should take this status into account
    /// (system: http://hl7.org/fhir/CodeSystem/medicationrequest-status)
    /// </summary>
    // [EnumLiteral("entered-in-error"), Description("Entered in Error")]
    EnteredInError,

    /// <summary>
    /// Actions implied by the prescription are to be permanently halted, before all of the administrations occurred. This should not be used if the original order was entered in error
    /// (system: http://hl7.org/fhir/CodeSystem/medicationrequest-status)
    /// </summary>
    // [EnumLiteral("stopped"), Description("Stopped")]
    Stopped,

    /// <summary>
    /// The prescription is not yet 'actionable', e.g. it is a work in progress, requires sign-off, verification or needs to be run through decision support process.
    /// (system: http://hl7.org/fhir/CodeSystem/medicationrequest-status)
    /// </summary>
    // [EnumLiteral("draft"), Description("Draft")]
    Draft,

    /// <summary>
    /// The authoring/source system does not know which of the status values currently applies for this observation. Note: This concept is not to be used for 'other' - one of the listed statuses is presumed to apply, but the authoring/source system does not know which.
    /// (system: http://hl7.org/fhir/CodeSystem/medicationrequest-status)
    /// </summary>
    // [EnumLiteral("unknown"), Description("Unknown")]
    Unknown,
}
