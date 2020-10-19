// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.ExceptionInterceptor;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.MvcHooks
{
    public class MsLoggerServiceExceptionInterceptor : IAsyncExceptionFilter
    {
        private readonly ILogger _Logger;

        public MsLoggerServiceExceptionInterceptor(ILogger<MsLoggerServiceExceptionInterceptor> logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task OnExceptionAsync(ExceptionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            _Logger.WriteExceptionFound(context.Exception.ToString());
            context.ExceptionHandled = false;
            return Task.CompletedTask;
        }
    }
}
