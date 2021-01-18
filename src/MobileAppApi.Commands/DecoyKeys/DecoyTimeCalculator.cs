﻿// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using MathNet.Numerics.Distributions;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.DecoyKeys
{
    public class DecoyTimeCalculator : IDecoyTimeCalculator
    {
        private readonly DecoyKeysLoggingExtensions _Logger;

        private int _Count;
        private double _SumSquareDiff;
        private readonly object _Lock = new object();

        public double DecoyTimeMean { get; private set; }
        public double DecoyTimeStDev => _Count > 1 ? Math.Sqrt(_SumSquareDiff / (_Count - 1)) : 0;

        public DecoyTimeCalculator(DecoyKeysLoggingExtensions logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _Count = 0;
            DecoyTimeMean = 0;
            _SumSquareDiff = 0;
        }

        public void RegisterTime(double timeMs) //Welford's algorithm
        {
            if (timeMs <= 0)
            {
                throw new ArgumentOutOfRangeException($"RegisterTime was called with a non-positive time: {timeMs}");
            }

            lock (_Lock)
            {
                _Count++;
                UpdateMeanAndSumSquareDiff(timeMs);
                _Logger.WriteTimeRegistered(_Count, timeMs, DecoyTimeMean, DecoyTimeStDev);
            }
        }

        public TimeSpan GenerateDelayTime()
        {
            lock (_Lock)
            {
                var gaussian = new Normal(DecoyTimeMean, DecoyTimeStDev);
                var delayMs = TimeSpan.FromMilliseconds(gaussian.Sample());
                
                _Logger.WriteGeneratingDelay(delayMs);
                return delayMs;
            }
        }

        private void UpdateMeanAndSumSquareDiff(double timeMs)
        {
            if (_Count == 1)
            {
                DecoyTimeMean = timeMs;
                return;
            }
            
            var newMean = DecoyTimeMean + (timeMs - DecoyTimeMean) / _Count;
            _SumSquareDiff += (timeMs - DecoyTimeMean) * (timeMs - newMean);
            DecoyTimeMean = newMean;
        }
    }
}
