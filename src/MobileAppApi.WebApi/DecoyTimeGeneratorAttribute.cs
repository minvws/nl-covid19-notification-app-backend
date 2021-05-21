// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.DecoyKeys;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi
{
    // NOTE: do not apply this attribute directly, apply DecoyTimeGeneratorAttributeFactory
    public class DecoyTimeGeneratorAttribute : IResourceFilter
    {
        private readonly IDecoyTimeCalculator _Calculator;
        
        public DecoyTimeGeneratorAttribute(IDecoyTimeCalculator calculator)
        {
            _Calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            var delayStopWatch = new Stopwatch();
            delayStopWatch.Start();

            while (delayStopWatch.Elapsed <= _Calculator.GetDelay())
            {
            }

            delayStopWatch.Stop();
        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
        }
    }
}