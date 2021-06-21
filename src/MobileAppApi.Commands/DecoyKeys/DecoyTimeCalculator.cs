// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Diagnostics;
namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.DecoyKeys
{
    public class DecoyTimeCalculator : IDecoyTimeCalculator
    {
        private sealed class DecoyTimeCalculatorHandle : IDisposable
        {
            private readonly Stopwatch _stopwatch = new Stopwatch();
            private readonly Action<TimeSpan> _register;

            public DecoyTimeCalculatorHandle(Action<TimeSpan> register)
            {
                _stopwatch.Start();
                _register = register ?? throw new ArgumentNullException(nameof(register));
            }

            public void Dispose()
            {
                _stopwatch.Stop();
                _register(_stopwatch.Elapsed);
            }
        }

        private readonly DecoyKeysLoggingExtensions _logger;
        private readonly object _lock = new object();
        private readonly IWelfordsAlgorithm _algorithm;

        public DecoyTimeCalculator(DecoyKeysLoggingExtensions logger, IWelfordsAlgorithm algorithm)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _algorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));
        }

        public IDisposable GetTimeRegistrationHandle() => new DecoyTimeCalculatorHandle(AddDataPoint);

        private void AddDataPoint(TimeSpan t)
        {
            if (t <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(t), "Must be positive.");
            }

            lock (_lock)
            {
                var result = _algorithm.AddDataPoint(t.TotalMilliseconds);
                _logger.WriteTimeRegistered(result.Count, t.TotalMilliseconds, result.Mean, result.StandardDeviation);
            }
        }

        public TimeSpan GetDelay()
        {
            lock (_lock)
            {
                var timeMs = _algorithm.GetNormalSample();
                var result = TimeSpan.FromMilliseconds(timeMs >= 0 ? timeMs : 0);
                _logger.WriteGeneratingDelay(result);
                return result;
            }
        }
    }
}
