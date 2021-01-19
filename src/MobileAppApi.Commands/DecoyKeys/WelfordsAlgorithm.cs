using System;
using MathNet.Numerics.Distributions;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.DecoyKeys
{
    public class WelfordsAlgorithm
    {

        private int _Count;
        private double _SumSquareDiff;
        private double _Mean;
        private double GetStdDev() => _Count > 1 ? Math.Sqrt(_SumSquareDiff / (_Count - 1)) : 0;

        public WelfordsAlgorithmState AddDataPoint(double amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException($"RegisterTime was called with a non-positive time: {amount}");
            }

            Update(amount);

            return new WelfordsAlgorithmState(_Count, _Mean, GetStdDev());
        }

        public double GetCurrent() => new Normal(_Mean, GetStdDev()).Sample();

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