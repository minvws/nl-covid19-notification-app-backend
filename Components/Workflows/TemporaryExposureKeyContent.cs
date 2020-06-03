// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows
{
    
    /// <summary>
    /// For Db content
    /// </summary>
    public class TemporaryExposureKeyContent
    {
        /// <summary>
        /// TemporaryExposureKey
        /// The 'ephemeral key' is called the RollingProximityIdentifier
        /// </summary>
        public string DailyKey { get; set; }

        /// <summary>
        /// Equivalent of Dp3T Epoch
        /// </summary>
        public int RollingStart { get; set; }

        /// <summary>
        /// Number of epochs in 24hours
        /// Currently fixed at 144? e.g. 10mins
        /// </summary>
        public int RollingPeriod { get; set; }

        /// <summary>
        /// Yet to be well defined.
        /// Self-diagnosis support?
        /// Comes from server?
        /// Phone can transmit risk level.
        /// 1-8 or 0-100?
        /// </summary>
        public int Risk { get; set; }
    }
}