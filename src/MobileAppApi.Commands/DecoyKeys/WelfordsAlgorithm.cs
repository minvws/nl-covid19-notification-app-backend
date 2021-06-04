// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using MathNet.Numerics.Distributions;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.DecoyKeys
{
    public class WelfordsAlgorithm : IWelfordsAlgorithm
    {
        private int _count;
        private double _sumSquareDiff;
        private double _mean;

        private double GetStdDev() => _count > 1 ? Math.Sqrt(_sumSquareDiff / (_count - 1)) : 0;

        public WelfordsAlgorithmState CurrentState => new WelfordsAlgorithmState(_count, _mean, GetStdDev());

        public WelfordsAlgorithmState AddDataPoint(double amount)
        {
            Update(amount);
            return CurrentState;
        }

        public double GetNormalSample() => new Normal(_mean, GetStdDev()).Sample();

        private void Update(double newAmount)
        {
            _count++;

            if (_count == 1)
            {
                _mean = newAmount;
                return;
            }

            var newMean = _mean + (newAmount - _mean) / _count;
            _sumSquareDiff += (newAmount - _mean) * (newAmount - newMean);
            _mean = newMean;
        }
    }
}