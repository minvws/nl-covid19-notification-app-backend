// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps
{
    public class HttpPostGenerateExposureKeySetsCommand
    {
        private readonly ExposureKeySetBatchJobMk2 _Job;

        public HttpPostGenerateExposureKeySetsCommand(ExposureKeySetBatchJobMk2 job)
        {
            _Job = job ?? throw new ArgumentNullException(nameof(job));
        }

        public async Task<IActionResult> Execute(bool useAllKeys = false)
        {
            await _Job.Execute(useAllKeys);
            return new OkResult();
        }
    }
}
