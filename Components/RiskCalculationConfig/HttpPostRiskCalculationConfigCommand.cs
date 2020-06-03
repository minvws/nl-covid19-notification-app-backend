// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig
{
    public class HttpPostRiskCalculationConfigCommand
    {
        private readonly RiskCalculationConfigInsertDbCommand _Writer;
        private readonly RiskCalculationConfigValidator _Validator;

        public HttpPostRiskCalculationConfigCommand(RiskCalculationConfigInsertDbCommand writer, RiskCalculationConfigValidator validator)
        {
            _Writer = writer;
            _Validator = validator;
        }

        public async Task<IActionResult> Execute(RiskCalculationConfigArgs args)
        {
            if (!_Validator.Valid(args))
                return new BadRequestResult();

            await _Writer.Execute(args);
            return new OkResult();
        }
    }
}