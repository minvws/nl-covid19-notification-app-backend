// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig
{
    
    public class HttpGetRiskCalculationConfigCommand
    {
        private readonly SafeGetRiskCalculationConfigDbCommand _SafeReadcommand;

        public HttpGetRiskCalculationConfigCommand(SafeGetRiskCalculationConfigDbCommand safeReadcommand)
        {
            _SafeReadcommand = safeReadcommand;
        }

        public IActionResult Execute(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return new BadRequestResult();

            //TODO validate the id further?

            var e = _SafeReadcommand.Execute(id.Trim());

            if (e == null)
                return new NotFoundResult();

            var content = JsonConvert.DeserializeObject<RiskCalculationConfigContent>(Encoding.UTF8.GetString(e.Content));
            var result = content.ToResponse();
            return new OkObjectResult(result);
        }
    }
}