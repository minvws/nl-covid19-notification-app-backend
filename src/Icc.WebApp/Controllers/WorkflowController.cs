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
        private const int OldGGDKeyLenght = 6;

        private readonly IRestApiClient _restApiClient;
        private readonly ILuhnModNGenerator _lLuhnModNGenerator;
        private readonly ILuhnModNValidator _luhnModNValidator;
        private readonly ILogger _logger;

        public WorkflowController(IServiceProvider serviceProvider, ILuhnModNGenerator luhnModNGenerator, ILuhnModNValidator luhnModNValidator, ILogger<WorkflowController> logger)
        {
            _restApiClient = serviceProvider.GetService<IRestApiClient>();
            _lLuhnModNGenerator = luhnModNGenerator ?? throw new ArgumentNullException(nameof(luhnModNGenerator));
            _luhnModNValidator = luhnModNValidator ?? throw new ArgumentNullException(nameof(luhnModNValidator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPut]
        [Route(EndPointNames.CaregiversPortalApi.PubTek)]
        public async Task<IActionResult> PutPubTek([FromBody] PublishTekArgs args)
        {
            if (_restApiClient == null) throw new ArgumentNullException(nameof(_restApiClient));

            var isValid = FixOrValidatePubTEK(args);

            if (!isValid)
            {
                var response = new PublishTekResponse
                {
                    Valid = false
                };
                return new OkObjectResult(response);
            }
            
            var source = new CancellationTokenSource();
            var token = source.Token;
            
            _logger.WriteLabStart();
            return await _restApiClient.PutAsync(args, $"{EndPointNames.CaregiversPortalApi.PubTek}", token);
        }
        
        [HttpGet]
        [AllowAnonymous]
        [Route("/AssemblyDump")]
        public IActionResult AssemblyDump([FromServices] IWebHostEnvironment env) => new DumpAssembliesToPlainText().Execute(env.IsDevelopment());


        private bool FixOrValidatePubTEK(PublishTekArgs args)
        {
            if (args.GGDKey.Length == OldGGDKeyLenght)
            {
                args.GGDKey = _lLuhnModNGenerator.CalculateCheckCode(args.GGDKey);
                return true;
            }

            return _luhnModNValidator.Validate(args.GGDKey);
        }
    }
}
