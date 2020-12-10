// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands
{

    public class WorkflowStats
    {
        public int Count { get; set; }
        public int TekCount { get; set; }
        public int Unauthorised { get; set; }
        public int Authorised { get; set; }
        public int AuthorisedAndFullyPublished { get; set; }
        public int TekPublished { get; set; }
        public int TekUnpublished { get; set; }
        public int Expired { get; set; }
    }
}