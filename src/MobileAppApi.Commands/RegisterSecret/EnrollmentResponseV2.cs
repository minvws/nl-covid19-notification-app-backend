// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.RegisterSecret
{
    public class EnrollmentResponseV2
    {
        // Key displayed by the phone for a user's interaction with the GGD
        public string GGDKey { get; set; }

        // Random generated number that will later in the upload process associate the keys with the correct signature
        public string BucketId { get; set; }

        public string ConfirmationKey { get; set; }

        public long Validity { get; set; }
    }
}