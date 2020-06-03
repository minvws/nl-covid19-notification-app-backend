// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows
{
    public class KeysFirstWorkflowEntity //: DatabaseEntity
    {
        public int Id { get; set; }
        public string AuthorisationToken { get; set; }

        /// <summary>
        /// False by default, authorized via CPS
        /// </summary>
        public bool Authorised { get; set; }
        public DateTime Created { get; set; }
        public string Content { get; set; }
        public string Region { get; set; } = DefaultValues.Region;
    }
}