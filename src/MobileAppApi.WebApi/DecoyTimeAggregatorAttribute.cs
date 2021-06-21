// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Mvc.Filters;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.DecoyKeys;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi
{
    public class DecoyTimeAggregatorAttribute : IResourceFilter
    {
        private readonly IDisposable _timeRegistrationHandle;

        public DecoyTimeAggregatorAttribute(IDecoyTimeCalculator calculator)
        {
            _timeRegistrationHandle = (calculator ?? throw new ArgumentNullException(nameof(calculator))).GetTimeRegistrationHandle();
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            _timeRegistrationHandle?.Dispose();
        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
        }
    }
}
