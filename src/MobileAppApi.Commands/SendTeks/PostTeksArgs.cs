// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.SendTeks
{
    /// <summary>
    /// Diagnosis key
    /// </summary>
    public class PostTeksArgs
    {
        /// <summary>
        /// byte array encoded as base64
        /// </summary>
        public string BucketId { get; set; }

        /// <summary>
        /// Teks
        /// </summary>
        public PostTeksItemArgs[] Keys { get; set; }

        /// <summary>
        /// Padding is provided by the frontend to control request sizes, it is ignored here further
        /// </summary>
        public string? Padding { get; set; }
    }
}

