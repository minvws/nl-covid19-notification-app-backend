// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using MathNet.Numerics.Distributions;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.DecoyKeys
{
    public class WelfordsAlgorithm : IWelfordsAlgorithm
    {
        private int _Count;
        private double _SumSquareDiff;
        private double _Mean;

        private double GetStdDev() => _Count > 1 ? Math.Sqrt(_SumSquareDiff / (_Count - 1)) : 0;

        public WelfordsAlgorithmState CurrentState => new WelfordsAlgorithmState(_Count, _Mean, GetStdDev());

        public WelfordsAlgorithmState AddDataPoint(double amount)
        {
            Update(amount);
            return CurrentState;
        }

        public double GetNormalSample() => new Normal(_Mean, GetStdDev()).Sample();

        private void Update(double newAmount)
        {
            _Count++;

            if (_Count == 1)
            {
                _Mean = newAmount;
                return;
            }

            var newMean = _Mean + (newAmount - _Mean) / _Count;
            _SumSquareDiff += (newAmount - _Mean) * (newAmount - newMean);
            _Mean = newMean;
        }
    }
}