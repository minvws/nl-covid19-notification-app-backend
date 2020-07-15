// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig
{
    public class HttpPostRiskCalculationConfigCommand
    {
        private readonly ExposureContentDbContext _ContextProvider;
        private readonly RiskCalculationConfigInsertDbCommand _Writer;
        private readonly RiskCalculationConfigValidator _Validator;
        private readonly ILogger _Logger;

        public HttpPostRiskCalculationConfigCommand(ExposureContentDbContext contextProvider, RiskCalculationConfigInsertDbCommand writer, RiskCalculationConfigValidator validator, ILogger logger)
        {
            _ContextProvider = contextProvider ?? throw new ArgumentNullException(nameof(contextProvider));
            _Writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _Validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Execute(RiskCalculationConfigArgs args)
        {
            if (!_Validator.Valid(args))
            {
                _Logger.LogWarning("Invalid args.");
                return new BadRequestResult();
            }

            _Logger.LogDebug("Writing DB.");
            await _Writer.Execute(args);
            _Logger.LogDebug("Committing.");
            _ContextProvider.SaveAndCommit();
            _Logger.LogInformation($"Committed.");
            return new OkResult();
        }
    }
}