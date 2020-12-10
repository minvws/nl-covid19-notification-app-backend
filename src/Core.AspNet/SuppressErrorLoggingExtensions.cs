// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet
{
    public class SuppressErrorLoggingExtensions
    {
        private const string Name = "SuppressError";

        private const int CallFailed = LoggingCodex.SuppressError;

        private readonly ILogger _Logger;

        public SuppressErrorLoggingExtensions(ILogger<SuppressErrorLoggingExtensions> logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WriteCallFailed(ActionDescriptor actionDescriptor)
        {
            if (actionDescriptor == null)
            { 
                throw new ArgumentNullException(nameof(actionDescriptor));
            }

            _Logger.LogDebug("[{name}/{id}] Call to {ActionDescriptor} failed, overriding response code to return 200.",
                Name, CallFailed,
                actionDescriptor);
        }

    }
}
