// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.RegisterSecret
{
    /// <summary>
    /// EnrollmentRequest
    /// </summary>
    public class KeysLastSecretArgs
    {
        /// <summary>
        /// EnrollmentRequest
        /// confirmationKey
        /// </summary>
        public string ConfirmationKey { get; set; }

        /// <summary>
        /// TODO Should this be consistently applied to the other posts/gets? Also byte[], please.
        /// </summary>
        public string Padding { get; set; }
    }
}