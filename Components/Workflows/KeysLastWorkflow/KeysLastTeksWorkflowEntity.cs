// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow
{
    public class KeyReleaseWorkflowState
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public string? LabConfirmationId { get; set; }
        public string? ConfirmationKey { get; set; }
        public string? BucketId { get; set; }
        public bool Authorised { get; set; }
        public ICollection<TemporaryExposureKeyEntity> Keys { get; set; }
    }
}
