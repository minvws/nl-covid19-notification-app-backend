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
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ICC;

namespace NL.Rijksoverheid.ExposureNotification.ICCBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GenerateICCController
    {
        private readonly ILogger<GenerateICCController> _logger;
        private readonly IConfiguration _Configuration;
        private readonly ICCBackendContentDbContext _context;

        public GenerateICCController(ILogger<GenerateICCController> logger, IConfiguration configuration, ICCBackendContentDbContext context)
        {
            _logger = logger;
            _Configuration = configuration;
            _context = context;
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
        [HttpPost]
        public async Task<ActionResult<object>> PostGenerateICC([FromBody] Guid UserId)
        {
            int length = Convert.ToInt32(_Configuration.GetSection("ICCConfig:Code:Length").Value);
            string generatedIcc = RandomString(length );
            InfectionConfirmationCodeEntity icc = new InfectionConfirmationCodeEntity();
            icc.Code = generatedIcc;
            icc.GeneratedBy = UserId;

             _context.InfectionConfirmationCodes.Add(icc);
             await _context.SaveChangesAsync();
            
            
            // icc.sacv

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