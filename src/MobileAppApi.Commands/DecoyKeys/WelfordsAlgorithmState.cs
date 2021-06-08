// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.DecoyKeys
{
    public class WelfordsAlgorithmState
    {
        public WelfordsAlgorithmState(int count, double mean, double standardDeviation)
        {
            Count = count;
            Mean = mean;
            StandardDeviation = standardDeviation;
        }

        public int Count { get; }
        public double Mean { get; }
        public double StandardDeviation { get; }
    }
}
