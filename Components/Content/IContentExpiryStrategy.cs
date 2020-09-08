// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Http;
using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public interface IContentExpiryStrategy
    {
        /// <summary>
        /// Calculate the time-to-live in seconds
        /// </summary>
        int Calculate(DateTime created);
    }
}