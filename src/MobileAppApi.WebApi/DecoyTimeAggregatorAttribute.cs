using System;
using Microsoft.AspNetCore.Mvc.Filters;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.DecoyKeys;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi
{
    public class DecoyTimeAggregatorAttribute : ActionFilterAttribute
    {
        private readonly IDisposable _Handle;

        public DecoyTimeAggregatorAttribute(IDecoyTimeCalculator calculator)
        {
            _Handle = (calculator ?? throw new ArgumentNullException(nameof(calculator))).GetTimeRegistrationHandle();
        }

        public override void OnResultExecuted(ResultExecutedContext context)
        {
            base.OnResultExecuted(context);
            _Handle?.Dispose();
        }
    }
}