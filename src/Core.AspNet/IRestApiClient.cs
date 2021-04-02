// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet
{
    public interface IRestApiClient
    {
        Uri BaseAddress { get; set; }
        Task<IActionResult> GetAsync(string requestUri, CancellationToken token);
        Task<IActionResult> PostAsync<T>(T model, string requestUri, CancellationToken token) where T : class;
        Task<IActionResult> PutAsync<T>(T model, string requestUri, CancellationToken token) where T : class;
    }
}
