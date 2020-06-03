// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows
{
    public enum TokenFirstWorkflowState
    {
        /// <summary>
        /// Starting state.
        /// Will accept OpenForAuthorisation only.
        /// Delete when expired.
        /// </summary>
        Unauthorised = 0,

        /// <summary>
        /// Can receive TEKs from mobile client.
        /// Return to Unauthorised when window expires.
        /// </summary>
        Receiving,

        /// <summary>
        /// Final state.
        /// Ready for next Exposure Key Set creation run and subsequent deletion.
        /// </summary>
        Authorised
    }
}