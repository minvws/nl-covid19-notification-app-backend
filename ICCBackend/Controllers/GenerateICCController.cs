// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ICC;

namespace NL.Rijksoverheid.ExposureNotification.ICCBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GenerateICCController
    {
        private readonly IICCService _ICCService;
        private readonly ILogger<GenerateICCController> _logger;
        private readonly IConfiguration _Configuration;
        

        public GenerateICCController(ILogger<GenerateICCController> logger, IConfiguration configuration, IICCService iccService )
        {
            _logger = logger;
            _Configuration = configuration;
            _ICCService = iccService;
        }


        [HttpPost("single")]
        public async Task<JsonResult> PostGenerateICC([FromBody] Guid userId)
        {
            InfectionConfirmationCodeEntity icc = await _ICCService.GenerateICC(userId);
            return new JsonResult(new
            {
                ok = true,
                status = 200,
                icc = icc,
                length = _Configuration.GetSection("ICCConfig:Code:Length").Value
            });
        }
        
        
        [HttpPost("batch")]
        public async Task<JsonResult> PostGenerateBatchICC([FromBody] Guid userId)
        {
            List<InfectionConfirmationCodeEntity> batch = await _ICCService.GenerateBatch(userId);
            return new JsonResult(new
            {
                ok = true,
                status = 200,
                length = _Configuration.GetSection("ICCConfig:Code:Length").Value,
                batch = batch
            });
        }
    }
}