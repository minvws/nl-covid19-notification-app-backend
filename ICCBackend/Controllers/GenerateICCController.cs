// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.ICCBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GenerateICCController
    {
        private readonly ILogger<GenerateICCController> _logger;
        private readonly IConfiguration _Configuration;

        public GenerateICCController(ILogger<GenerateICCController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _Configuration = configuration;
        }
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // [HttpGet]
        // public async Task<ActionResult<object>> GetGenerateICCBatch()
        // {
        //     throw new NotImplementedException();;
        // }
        [HttpGet]
        public async Task<ActionResult<object>> GetGenerateICC()
        {
            int length = Convert.ToInt32(_Configuration.GetSection("ICCConfig:Code:Length").Value);
            string generatedIcc = RandomString(length );
            // int generatedIcc = RandomNumberGenerator.GetInt32(Convert.ToInt32("1" + new String('0', length-2)), Convert.ToInt32(new String('9', length)));
            
            return new JsonResult(new
            {
                ok = true,
                status = 200,
                icc = generatedIcc,
                length = _Configuration.GetSection("ICCConfig:Code:Length").Value
            });
        }
    }
}