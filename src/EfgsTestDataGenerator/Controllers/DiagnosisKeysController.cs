using System;
using System.IO;
using System.Net;
using EfgsTestDataGenerator.Services;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EfgsTestDataGenerator.Controllers
{
    [Route("diagnosiskeys")]
    public class DiagnosisKeysController : Controller
    {
        private readonly EfgsDataService _efgsDataService;
        private readonly ILogger<DiagnosisKeysController> _logger;

        public DiagnosisKeysController(EfgsDataService efgsDataService, ILogger<DiagnosisKeysController> logger)
        {
            _efgsDataService = efgsDataService ?? throw new ArgumentNullException(nameof(efgsDataService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("download/{date}")]
        public IActionResult DownloadToIActionResult(string date)
        {
            HttpContext.Request.Headers.TryGetValue("batchTag", out var batchTag);

            var efgsDataSet = _efgsDataService.GetEfgsDataSet(date, batchTag);

            if(efgsDataSet == null)
            {
                return StatusCode((int)HttpStatusCode.Gone);
            }

            var memorystream = new MemoryStream(efgsDataSet.Content.ToByteArray());
            var response = Ok(memorystream);

            Response.Headers.Add("batchTag", efgsDataSet.BatchTag);
            Response.Headers.Add("nextBatchTag", efgsDataSet.NextBatchTag);

            return response;
        }
    }
}
