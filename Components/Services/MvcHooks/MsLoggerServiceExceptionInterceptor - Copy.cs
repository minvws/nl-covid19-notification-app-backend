using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.MvcHooks
{

    public class SerilogServiceExceptionInterceptor : IAsyncExceptionFilter
    {
        private readonly ILogger _Logger;

        public SerilogServiceExceptionInterceptor(ILogger logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task OnExceptionAsync(ExceptionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            _Logger.Error(context.Exception.ToString());
            context.ExceptionHandled = false;
            return Task.CompletedTask;
        }
    }
}
