// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.RegisterSecret
{
    public class EnrollmentResponse
    {
        // Confirmation code that the phone must display so the user can read it to an operator.
        public string LabConfirmationId { get; set; }

        // Random generated number that will later in the upload process associate the keys with the correct signature
        public string BucketId { get; set; }

        public string ConfirmationKey { get; set; }

        public long Validity { get; set; }
    }
}
