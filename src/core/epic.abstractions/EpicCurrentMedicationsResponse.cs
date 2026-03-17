// ******************
// © 2025 MCG Health.
// ******************

using Mcg.Edge.Fhir.Epic.Abstractions;

namespace epic.medications;

public class EpicCurrentMedicationsResponse
{
    public IEnumerable<MedicationOrder> MedicationOrders { get; set; }
}
