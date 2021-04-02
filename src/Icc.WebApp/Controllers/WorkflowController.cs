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
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.LuhnModN;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation.Handlers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.TekPublication;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.Icc.WebApp.Controllers
{
    [Authorize(AuthenticationSchemes = JwtAuthenticationHandler.SchemeName)]
    public class WorkflowController : Controller
    {
        private readonly IRestApiClient _restApiClient;
        private readonly ILogger _Logger;

        public WorkflowController(IServiceProvider serviceProvider, ILogger<WorkflowController> logger)
        {
            _restApiClient = serviceProvider.GetService<IRestApiClient>();
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPut]
        [Route(EndPointNames.CaregiversPortalApi.PubTek)]
        public async Task<IActionResult> PutAuthorise([FromBody] PublishTekArgs args, [FromServices] ILuhnModNValidator luhnModNValidator)
        {
            if (_restApiClient == null) throw new ArgumentNullException(nameof(_restApiClient));
            if(luhnModNValidator == null) throw new ArgumentNullException(nameof(luhnModNValidator));

            if (!luhnModNValidator.Validate(args.GGDKey))
            {
                var response = new PublishTekResponse
                {
                    Valid = false
                };
                return new OkObjectResult(response);
            }
            
            var source = new CancellationTokenSource();
            var token = source.Token;
            
            _Logger.WriteLabStart();
            return await _restApiClient.PutAsync(args, $"{EndPointNames.CaregiversPortalApi.PubTek}", token);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("/AssemblyDump")]
        public IActionResult AssemblyDump([FromServices] IWebHostEnvironment env) => new DumpAssembliesToPlainText().Execute(env.IsDevelopment());
    }
}
