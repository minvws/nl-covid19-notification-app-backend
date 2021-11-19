using System;
using System.Net;
using System.Net.Http;
using EfgsTestDataGenerator.Services;
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

        [Route("download/{date}")]
        public HttpResponseMessage Download(string date)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var batchTag = "";
            var nextBatchTag = "";

            response.Headers.Add("batchTag", batchTag);
            response.Headers.Add("nextBatchTag", nextBatchTag);

            return response;
        }
    }
}
