﻿// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
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
            private readonly Stopwatch _Stopwatch = new Stopwatch();
            private readonly Action<TimeSpan> _Register;

            public DecoyTimeCalculatorHandle(Action<TimeSpan> register)
            {
                _Stopwatch.Start();
                _Register = register ?? throw new ArgumentNullException(nameof(register));
            }

            public void Dispose()
            {
                _Stopwatch.Stop();
                _Register(_Stopwatch.Elapsed);
            }
        }

        private readonly DecoyKeysLoggingExtensions _Logger;
        private readonly object _Lock = new object();
        private readonly WelfordsAlgorithm _Algorithm = new WelfordsAlgorithm();

        public DecoyTimeCalculator(DecoyKeysLoggingExtensions logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IDisposable GetTimeRegistrationHandle() => new DecoyTimeCalculatorHandle(AddDataPoint);

        private void AddDataPoint(TimeSpan t)
        {
            if (t <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(t), "Must be positive.");
            }

            lock (_Lock)
            {
                var result = _Algorithm.AddDataPoint(t.Milliseconds);
                _Logger.WriteTimeRegistered(result.Count, t.Milliseconds, result.Mean, result.StandardDeviation); //TODO This would alter the actual total time?
            }
        }

        public TimeSpan GetDelay()
        {
            lock (_Lock)
            {
                var result = TimeSpan.FromMilliseconds(_Algorithm.GetNormalSample());
                _Logger.WriteGeneratingDelay(result);
                return result;
            }
        }
    }
}
