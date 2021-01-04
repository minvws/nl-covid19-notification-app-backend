// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet
{
    /// <summary>
    /// Suppresses error responses, replacing them with 200 OK
    /// </summary>
    public class SuppressErrorAttribute : ActionFilterAttribute
    {
        private readonly SuppressErrorLoggingExtensions _Logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        public SuppressErrorAttribute(SuppressErrorLoggingExtensions logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Adds a random padding as an http header to the response.
        /// </summary>
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            base.OnResultExecuting(context);
            
            if (!(context.Result is IStatusCodeActionResult statusCodeResult)) return;

            if (!statusCodeResult.StatusCode.HasValue || statusCodeResult.StatusCode.Value == 200) return;

            _Logger.WriteCallFailed(context.ActionDescriptor);
            context.Result = new OkResult();
        }
    }
}