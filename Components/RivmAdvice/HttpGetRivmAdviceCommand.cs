// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RivmAdvice
{
    public class HttpGetRivmAdviceCommand
    {
        private readonly SafeGetRivmAdviceCommand _SafeReadCommand;

        public HttpGetRivmAdviceCommand(SafeGetRivmAdviceCommand safeReadCommand)
        {
            _SafeReadCommand = safeReadCommand;
        }

        public IActionResult Execute(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return new BadRequestResult();

            var e = _SafeReadCommand.Execute(id.Trim());

            if (e == null)
                return new NotFoundResult();

            var content = JsonConvert.DeserializeObject<MobileDeviceRivmAdviceConfigEntityContent>(Encoding.UTF8.GetString(e.Content));

            var result = new RivmAdviceResponse
            {
                Id = id,
                IsolationPeriodDays = content.IsolationPeriodDays,
                TemporaryExposureKeyRetentionDays = content.TemporaryExposureKeyRetentionDays,
                ObservedTemporaryExposureKeyRetentionDays = content.ObservedTemporaryExposureKeyRetentionDays,
                Text = content.Text
            };

            return new OkObjectResult(result);
        }
    }
}