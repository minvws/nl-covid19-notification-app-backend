// Copyright ©  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows
{
    public class TekReleaseWorkflowEntity
    {
        public int Id { get; set; }

        /// <summary>
        /// Base time for the job to delete if never authorised.
        /// E.g. negative test?
        /// TODO Do we have/need a -ve test explicit delete?
        /// </summary>
        public DateTime Created { get; set; }
        public bool Authorised { get; set; }

        /// <summary>
        /// TEK[] as JSON
        /// </summary>
        public string TekContent { get; set; }

        public string Region { get; set; } = DefaultValues.Region;
    }
}