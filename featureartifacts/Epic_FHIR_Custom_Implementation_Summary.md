# Epic EHR-Specific FHIR Custom API Implementation Summary

## Overview
This document provides a comprehensive summary of Epic-specific custom FHIR API implementations in the system. These customizations extend beyond standard FHIR endpoints to leverage Epic's proprietary APIs for medication-related data retrieval.

## Epic Custom API Calls

### 1. GetCurrentMedications (MedicationRequest)
**Purpose**: Retrieves current medication orders for a patient from Epic's custom API

**Epic API Endpoint Pattern**:
```
{baseUrl}/api/epic/2014/Clinical/Patient/GETMEDICATIONSV2/GetCurrentMedications
```

**Request Model**: `EpicCurrentMedicationsRequest`
```csharp
{
    "patientID": string,                                      // Patient identifier
    "patientIDType": "FHIR",                                  // Always "FHIR" (from EpicConstants)
    "userID": string,                                         // Optional user identifier
    "userIDType": string,                                     // Optional user ID type
    "profileView": 2,                                         // Always 2 (from EpicConstants.ProfileView)
    "numberDaysToIncludeDiscontinuedAndEndedOrders": int      // Calculated from effective-time/authoredon criteria
}
```

**Response Model**: `EpicCurrentMedicationsResponse`
- Contains `MedicationOrders` array with order information
- Each order includes IDs, medication names, and status information

**Implementation Files**:
- **Workflow Step**: `CachedCallCustomEpicGetCurrentMedications.cs`
- **Client Method**: `IFhirClient.GetCurrentMedicationsAsync()`
- **Location**: `/FHIR/Workflows/Server/Steps/CachedCallCustomEpicGetCurrentMedications.cs`

**Key Logic**:
- Extracts patient ID from criteria or InitializeRequest
- Calculates lookback period from `authoredon` or `effective-time` FHIR search parameters
- Uses ProfileView=2 to get specific medication view
- Caches results per session to avoid duplicate calls
- Result is cached with key: `CachedCallCustomEpicGetCurrentMedications_{SessionId}`

---

### 2. GetMedicationAdministrationHistory (MedicationAdministration)
**Purpose**: Retrieves medication administration history for specific orders during an encounter

**Epic API Endpoint Pattern**:
```
{baseUrl}/api/epic/2014/Clinical/Patient/MEDICATIONADMINISTRATION/GetMedicationAdministrationHistory
```

**Request Model**: `EpicMedicationAdministrationRequest`
```csharp
{
    "patientID": string,           // Patient identifier
    "patientIDType": "FHIR",      // Always "FHIR" (from EpicConstants)
    "contactID": string,           // Encounter/Contact ID from Encounter.identifier.value
    "contactIDType": string,       // Usually "CSN" (from EpicConstants.DefaultContactType or Encounter.identifier.type.coding.code)
    "orderIDs": [                  // Array of order IDs from GetCurrentMedications response
        {
            "id": string,
            "type": "Internal"
        }
    ]
}
```

**Response Model**: `EpicMedicationAdministrationResponse`
```csharp
{
    "Orders": [
        {
            "orderID": { "id": string, "type": string },
            "name": string,
            "isActive": bool,
            "isInfusion": bool,
            "isMixture": bool,
            "linkedOrderIDs": [...],
            "linkedOrderType": string,
            "medicationAdministrations": [
                {
                    "action": string,
                    "administrationInstant": DateTimeOffset,
                    "dose": { "value": string, "unit": string },
                    "rate": { "value": string, "unit": string },
                    "mappedAction": string,
                    "linkedOverrideOrderID": [...]
                }
            ]
        }
    ]
}
```

**Implementation Files**:
- **Workflow Step**: `CustomGetMedicationAdministrationHistory.cs`
- **Mapper**: `MapTransformationResult_EpicMedicationAdministrationRequest.cs`
- **Client Method**: `IFhirClient.GetCurrentMedicationsHistoryAsync()`
- **Location**: `/FHIR/Workflows/Server/Steps/CustomGetMedicationAdministrationHistory.cs`

**Key Logic**:
- Requires prior calls to GetCurrentMedications and FHIR Encounter
- Extracts OrderIDs from cached GetCurrentMedications response (`MedicationOrders[].IDs[].ID`)
- Extracts ContactID and ContactIDType from FHIR Encounter resource
- If Encounter is a Bundle, filters by encounter ID from InitializeRequest
- Falls back to `EpicConstants.DefaultContactType` ("CSN") if type not specified
- Returns detailed administration events for each order

---

## Custom Workflows

### 1. EpicMedicationRequestWorkflow
**Purpose**: Custom workflow for Epic MedicationRequest resources

**Workflow Registration**:
```csharp
[Transform("EpicMedicationRequestWorkflow", "This is a custom workflow for doing the medication request resource type.")]
```

**Step Sequence**:
1. `CachedCallFhirMedicationRequest` - Standard FHIR MedicationRequest call
2. `CachedCallCustomEpicGetCurrentMedications` - **Epic custom call**
3. `CachedCallBuildMedicationRequests` - Build/merge medication data
4. `FhirResponse` - Return response

**File**: `/FHIR/Workflows/Server/EpicMedicationRequestWorkflow.cs`

---

### 2. EpicMedicationAdministrationWorkflow
**Purpose**: Custom workflow for Epic MedicationAdministration resources

**Workflow Registration**:
```csharp
[Transform("EpicMedicationAdministrationWorkflow", "This is a custom workflow for doing the medication administration request resource type.")]
```

**Step Sequence**:
1. `CachedCallFhirMedicationRequest` - Standard FHIR MedicationRequest call
2. `CachedCallCustomEpicGetCurrentMedications` - **Epic custom call #1**
3. `CachedCallBuildMedicationRequests` - Build medication data
4. `CachedCallFhirEncounter` - Get encounter information
5. `CustomGetMedicationAdministrationHistory` - **Epic custom call #2**
6. `BuildMedicationAdministration` - Build final response
7. `FhirResponse` - Return response

**File**: `/FHIR/Workflows/Server/EpicMedicationAdministrationWorkflow.cs`

---

## Configuration and Constants

### EpicConstants
**File**: `/FHIR/EpicConstants.cs`

```csharp
public abstract class EpicConstants
{
    public const string DefaultContactType = "CSN";    // Default contact ID type
    public const string PatientIDType = "FHIR";       // Patient ID type for Epic calls
    public const int ProfileView = 2;                  // Profile view for medication requests
}
```

### Configuration Keys (TransformAndMapCustomerFhirKVs)

**Epic API URLs Configuration**:
- `EpicBaseUrl` or derived from `InitializeRequest.Iss`
- `MedicationRequest_ISS` - Path for GetCurrentMedications endpoint
- `MedicationAdministration_ISS` - Path for GetMedicationAdministrationHistory endpoint

**Workflow Mapping**:
- Configuration allows mapping specific resource types to custom workflows per tenant
- Key pattern: `CustomerWorkflowsToResourceTypes.{ResourceType}` → Workflow Name
- Example: `MedicationRequest` → `EpicMedicationRequestWorkflow`

**File**: `/FHIR/TransformAndMapCustomerFhirKVs.cs`

**Method**: `EpicCustomApiUrl(FhirResourceType resourceType)`
- Constructs full Epic API URL by combining base URL and resource-specific path
- Falls back to extracting base URL from FHIR ISS if not explicitly configured

---

## HTTP Client Implementation

### IFhirClient Interface
**File**: `/FHIR/Abstractions/IFhirClient.cs`

```csharp
public interface IFhirClient
{
    Task<string> GetFhirResourceAsync(string queryString, string accessToken, CancellationToken cancellationToken);

    Task<string> GetCurrentMedicationsAsync(
        string baseUrl,
        EpicCurrentMedicationsRequest body,
        string accessToken,
        CancellationToken cancellationToken);

    Task<string> GetCurrentMedicationsHistoryAsync(
        string baseUrl,
        EpicMedicationAdministrationRequest body,
        string accessToken,
        CancellationToken cancellationToken);
}
```

### FhirClient Implementation
**File**: `/FHIR/Clients/FhirClient.cs`

**Authentication**:
- Uses OAuth2 Bearer token authentication
- Token passed from InitializeRequest.Access_token

**Headers**:
- `Authorization: Bearer {token}`
- `Content-Type: application/json`
- `Accept: application/fhir+json`

**HTTP Method**: POST for both Epic custom calls

**Error Handling**:
- Throws `ApiException` for non-success responses
- Includes PHI logging for request/response details

---

## Data Flow

### MedicationRequest Flow
```
1. Receive FHIR MedicationRequest search request
2. Extract patient ID and authoredon criteria
3. Call standard FHIR MedicationRequest endpoint (optional)
4. Call Epic GetCurrentMedications API
   - Calculate lookback days from authoredon
   - Use ProfileView=2, PatientIDType=FHIR
5. Cache Epic response
6. Build/merge medication request data
7. Return FHIR-compliant response
```

### MedicationAdministration Flow
```
1. Receive FHIR MedicationAdministration search request
2. Extract patient ID and effective-time criteria
3. Call standard FHIR MedicationRequest endpoint (optional)
4. Call Epic GetCurrentMedications API
   - Calculate lookback days from effective-time
   - Cache response with medication order IDs
5. Call standard FHIR Encounter endpoint
   - Extract encounter ID from InitializeRequest
   - Filter bundle if needed
   - Cache encounter data
6. Map data to EpicMedicationAdministrationRequest
   - Extract OrderIDs from cached medications
   - Extract ContactID and ContactIDType from encounter
   - Use PatientID from criteria or InitializeRequest
7. Call Epic GetMedicationAdministrationHistory API
8. Build FHIR MedicationAdministration resources
9. Return FHIR-compliant Bundle response
```

---

## Key Implementation Details

### Caching Strategy
- Both Epic calls use `AbstractCachedStep` pattern
- Cache key includes SessionId to isolate per-session
- Prevents duplicate API calls within same session
- Cached data accessed via `TransformationResult.AdditionalProperties`

### Patient ID Resolution
Priority order:
1. From search criteria parameter `patient`
2. From `InitializeRequest.Patient`

### Encounter ID Handling
- Supports both single Encounter and Bundle responses
- Filters Bundle by encounter ID from InitializeRequest
- Validates identifier presence before Epic call
- Extracts contact type from Encounter.identifier.type.coding.code
- Falls back to "CSN" if type not provided

### Medication Lookback Calculation
```csharp
// From authoredon (MedicationRequest) or effective-time (MedicationAdministration)
// Format: "ge2024-01-01" or "2024-01-01"
// Strips "ge"/"le" prefixes
// Calculates: (DateTime.UtcNow - effectiveDate).Days
// Returns 0 if not specified or invalid
```

### Order ID Extraction
```csharp
// From Epic GetCurrentMedications response:
MedicationOrders[].IDs[].ID
// Mapped to:
{ "id": "{extracted_id}", "type": "Internal" }
```

---

## Testing Considerations

### Test Data Files
- `R4FHIRMedicationRequestCustomEpicMedicAdmin*.json` - Test request payloads
- Located in `/Acceptance/Features/TestData/FHIR/`

### Test Scenarios
1. Valid medication administration request
2. No contact ID in encounter
3. No order IDs found
4. Invalid tenant configuration
5. Multiple medication orders
6. Bundle vs single encounter responses

### Mock/WireMock Support
- MockFhirClient implements IFhirClient
- WireMock responses in `/Domain/Clients/WireMock/FHIR/Responses/Medication/`
- Performance test support via `MedicationRequestPerfTest.json`

---

## Migration Guide for New System Version

### 1. Configuration Migration
**Required Settings**:
```json
{
  "EpicBaseUrl": "https://{epic-server}/",
  "MedicationRequest_ISS": "/api/epic/2014/Clinical/Patient/GETMEDICATIONSV2/GetCurrentMedications",
  "MedicationAdministration_ISS": "/api/epic/2014/Clinical/Patient/MEDICATIONADMINISTRATION/GetMedicationAdministrationHistory",
  "CustomerWorkflowsToResourceTypes": {
    "MedicationRequest": "EpicMedicationRequestWorkflow",
    "MedicationAdministration": "EpicMedicationAdministrationWorkflow"
  }
}
```

### 2. Required Models
Copy/regenerate these model classes:
- `EpicCurrentMedicationsRequest`
- `EpicCurrentMedicationsResponse`
- `EpicMedicationAdministrationRequest`
- `EpicMedicationAdministrationResponse`
- `EpicMedicationAdminOrder`
- `EpicMedicationAdministration`
- `IdType`
- `UnitValue`

### 3. Constants
Create equivalent of `EpicConstants`:
- `DefaultContactType = "CSN"`
- `PatientIDType = "FHIR"`
- `ProfileView = 2`

### 4. HTTP Client Extensions
Implement two new methods in FHIR client:
- `GetCurrentMedicationsAsync(url, request, token)` - POST with JSON body
- `GetCurrentMedicationsHistoryAsync(url, request, token)` - POST with JSON body

### 5. Workflow Steps
Implement these workflow steps:
- **CachedCallCustomEpicGetCurrentMedications**
  - Calculate medication lookback from search criteria
  - Call Epic API with ProfileView=2
  - Cache response
- **CustomGetMedicationAdministrationHistory**
  - Map from cached data (medications + encounter)
  - Extract OrderIDs and ContactID
  - Call Epic API
  - Return JSON response

### 6. Mappers
Implement:
- **MapTransformationResult_EpicMedicationAdministrationRequest**
  - Extract OrderIDs from cached medication response
  - Extract ContactID/Type from cached encounter
  - Handle Bundle vs single encounter
  - Validate required fields

### 7. Response Builders
- Build FHIR MedicationRequest from Epic medication orders
- Build FHIR MedicationAdministration from Epic admin history
- Map Epic actions to FHIR status/categories

### 8. Dependencies
- Access to InitializeRequest.Access_token for authentication
- Access to InitializeRequest.Patient and Encounter IDs
- Caching mechanism for workflow steps
- Configuration service for tenant-specific settings

### 9. Testing Requirements
- Unit tests for each workflow step
- Integration tests with Epic sandbox
- Mock/stub Epic API responses
- Performance tests with large result sets
- Error handling tests (missing data, API failures)

---

## Security Considerations

### Authentication
- Epic API calls use OAuth2 Bearer token from SMART on FHIR launch
- Token must have appropriate scopes for patient medication data
- Token passed from launch context, not stored

### PHI Logging
- Uses `IPHILogger` for request/response bodies
- Regular logger for non-PHI operational data
- Ensure Epic API responses treated as PHI

### Validation
- Validates all required fields before Epic API calls
- Throws validation exceptions for missing patient/encounter data
- Validates encounter has identifier before proceeding

---

## Performance Optimizations

### Caching
- Session-level caching prevents duplicate API calls
- GetCurrentMedications called once, shared between workflows
- Encounter data cached when used by multiple steps

### Parallel Execution
- FHIR and Epic calls could be parallelized where possible
- Current implementation is sequential for data dependency

### Batch Processing
- Order IDs sent as array in single API call
- Avoids N+1 problem for multiple medications

---

## Known Limitations

1. **Epic API Versioning**: Hardcoded to 2014 version paths
2. **Contact Type Flexibility**: Limited to types Epic supports (CSN, etc.)
3. **Profile View**: Fixed at ProfileView=2, may need configuration
4. **Encounter Dependency**: MedicationAdministration requires encounter context
5. **Time Calculation**: Simple day-based lookback, not precise datetime

---

## References

### Key Files
- `/FHIR/EpicConstants.cs`
- `/FHIR/Workflows/Server/EpicMedicationRequestWorkflow.cs`
- `/FHIR/Workflows/Server/EpicMedicationAdministrationWorkflow.cs`
- `/FHIR/Workflows/Server/Steps/CachedCallCustomEpicGetCurrentMedications.cs`
- `/FHIR/Workflows/Server/Steps/CustomGetMedicationAdministrationHistory.cs`
- `/FHIR/Mapping/Mcg.Edge.Domain.TransformAndMap.Mapping.FHIR/MapTransformationResult_EpicMedicationAdministrationRequest.cs`
- `/FHIR/TransformAndMapCustomerFhirKVs.cs`
- `/FHIR/Abstractions/IFhirClient.cs`
- `/FHIR/Clients/FhirClient.cs`
- `/FHIR/FhirServiceModel.cs`

### Epic API Documentation
Refer to Epic's documentation for:
- GetCurrentMedications API specification
- GetMedicationAdministrationHistory API specification
- Authentication requirements
- Data models and field definitions

---

## Summary

The Epic custom implementation provides two main API integrations:

1. **GetCurrentMedications**: Retrieves medication orders with Epic-specific details beyond standard FHIR MedicationRequest
2. **GetMedicationAdministrationHistory**: Retrieves detailed administration events for specific orders within an encounter context

Both require:
- FHIR patient ID
- OAuth2 authentication
- Epic-specific parameters (ProfileView, ContactType, etc.)
- Proper workflow orchestration with caching

The implementation uses a workflow-based architecture with cached steps, allowing reuse of API call results across multiple resource transformations within the same session.
