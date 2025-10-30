// <copyright file="EdgeSystem.cs" company="Hearst Company">
// Copyright (c) Hearst Company. All rights reserved.
// </copyright>

namespace FhirService.Plugin
{
    /// <summary>
    /// Constants common to all services.
    /// </summary>
    public abstract class EpicConstants
    {
        /// <summary>
        /// Gets the DefaultContactType.
        /// </summary>
        /// <value>
        /// DefaultContactType to make Epic custom MedicationAdministration calls.
        /// </value>
        public const string DefaultContactType = "CSN";

        /// <summary>
        /// Gets the Epic PatientIdType.
        /// </summary>
        /// <value>
        /// PatientIdType to make Epic custom calls.
        /// </value>
        public const string PatientIDType = "FHIR";

        /// <summary>
        /// Gets the Epic ProfileView.
        /// </summary>
        /// <value>
        /// ProfileView to make Epic custom MedicationRequest calls.
        /// </value>
        public const int ProfileView = 2;
    }
}
