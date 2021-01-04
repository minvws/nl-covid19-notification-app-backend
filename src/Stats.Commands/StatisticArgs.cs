// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.Commands
{
    public class StatisticArgs
    {
        public string Name { get; set; }
        public string Qualifier { get; set; }
        public double Value { get; set; }
    }
}
