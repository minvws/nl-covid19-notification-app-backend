using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation.Handlers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.Icc.WebApp.Controllers
{
    [Authorize(AuthenticationSchemes = JwtAuthenticationHandler.SchemeName)]
    public class WorkflowController : Controller
    {
        private readonly IRestApiClient _RestApiClient;
        private readonly ILogger _Logger;

        public WorkflowController(IServiceProvider serviceProvider, ILogger<WorkflowController> logger)
        {
            _RestApiClient = serviceProvider.GetService<IRestApiClient>();
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [Route(EndPointNames.CaregiversPortalApi.LabConfirmation)]
        public async Task<IActionResult> PostAuthorise([FromBody] AuthorisationArgs args)
        {
            if (_RestApiClient == null) throw new ArgumentNullException(nameof(_RestApiClient));

            var source = new CancellationTokenSource();
            var token = source.Token;

            _Logger.WriteLabStart();
            var result = await _RestApiClient.PostAsync(args, $"{EndPointNames.CaregiversPortalApi.LabConfirmation}", token);

            return Ok();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("/AssemblyDump")]
        public IActionResult AssemblyDump([FromServices] IWebHostEnvironment env) => new DumpAssembliesToPlainText().Execute(env.IsDevelopment());
    }
}
