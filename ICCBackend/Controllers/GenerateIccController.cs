// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ICC.Services;
using NL.Rijksoverheid.ExposureNotification.IccBackend.Models;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.IccBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GenerateIccController
    {
        private readonly IIccService _IccService;
        private readonly ILogger<GenerateIccController> _Logger;
        private readonly IConfiguration _Configuration;
        private readonly IccBackendContentDbContext _DbContext;


        public GenerateIccController(ILogger<GenerateIccController> logger, IConfiguration configuration,
            IIccService iccService, IccBackendContentDbContext dbContext)
        {
            _Logger = logger;
            _Configuration = configuration;
            _IccService = iccService;
            _DbContext = dbContext;
        }


        [HttpPost("single")]
        public async Task<JsonResult> PostGenerateIcc(GenerateIccInputModel generateIccInputModel)
        {
            var infectionConfirmationCodeEntity =
                await _IccService.GenerateIcc(generateIccInputModel.UserId, null!);
            _DbContext.SaveAndCommit();
            return new JsonResult(new
            {
                ok = true,
                status = 200,
                icc = infectionConfirmationCodeEntity,
                length = _Configuration.GetSection("IccConfig:Code:Length").Value
            });
        }

        [HttpPost("batch")]
        public async Task<JsonResult> PostGenerateBatchIcc(GenerateIccInputModel generateIccInputModel)
        {
            var iccBatch = await _IccService.GenerateBatch(generateIccInputModel.UserId);
            _DbContext.SaveAndCommit();
            return new JsonResult(new
            {
                ok = true,
                status = 200,
                length = _Configuration.GetSection("IccConfig:Code:Length").Value,
                iccBatch = iccBatch
            });
        }

        [HttpPost("batch-csv")]
        public async Task<FileContentResult> PostGenerateCsv(GenerateIccInputModel generateIccInputModel)
        {
            var iccBatch = await _IccService.GenerateBatch(generateIccInputModel.UserId);

            _DbContext.SaveAndCommit();

            byte[] content;

            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.GetCultureInfo("nl-NL")))
            {    
                csvWriter.WriteRecords(iccBatch.Batch);
                streamWriter.Flush();
                content = memoryStream.ToArray();
            }

            return new FileContentResult(content, "text/csv")
            {
                FileDownloadName = "icc.csv"
            };
        }

    }
}