// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.DecoyKeys
{
    public interface IWelfordsAlgorithm
    {
        WelfordsAlgorithmState CurrentState { get; }

        WelfordsAlgorithmState AddDataPoint(double amount);

        public double GetNormalSample();
    }
}
