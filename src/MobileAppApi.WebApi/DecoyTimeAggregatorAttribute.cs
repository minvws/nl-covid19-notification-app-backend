using System;
using Microsoft.AspNetCore.Mvc.Filters;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.DecoyKeys;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi
{
    public class DecoyTimeAggregatorAttribute : IResourceFilter
    {
        private readonly IDisposable _TimeRegistrationHandle;

        public DecoyTimeAggregatorAttribute(IDecoyTimeCalculator calculator)
        {
            _TimeRegistrationHandle = (calculator ?? throw new ArgumentNullException(nameof(calculator))).GetTimeRegistrationHandle();
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            _TimeRegistrationHandle?.Dispose();
        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
        }
    }
}