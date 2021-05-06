using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
        private const int OldGGDKeyLength = 6;
        private const int ValidGGDKeyLength = 7;

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
        [Route("/pubtek")]
        public async Task<PublishTekResponse> PutPubTek([FromBody] PublishTekArgs args)
        {
            if (_restApiClient == null) throw new ArgumentNullException(nameof(_restApiClient));

            // Fail fast -> If the code is not valid, return the response with false result. 
            if (!FixOrValidatePubTEK(args))
            {
                return new PublishTekResponse { Valid = false };
            }

            var source = new CancellationTokenSource();
            var token = source.Token;

            _logger.WritePubTekStart();
            var responseMessage = await _restApiClient.PutAsync(args, $"{EndPointNames.CaregiversPortalApi.PubTek}", token);
            var result = JsonConvert.DeserializeObject<PublishTekResponse>(await responseMessage.Content.ReadAsStringAsync());
            return result;
        }


        [HttpGet]
        [AllowAnonymous]
        [Route("/AssemblyDump")]
        public IActionResult AssemblyDump([FromServices] IWebHostEnvironment env) => new DumpAssembliesToPlainText().Execute(env.IsDevelopment());


        private bool FixOrValidatePubTEK(PublishTekArgs args)
        {
            // If a 6 digit code has been send no LuhnModN validation is possible at this point. Just add the check code and return valid.
            if (args.GGDKey.Length == OldGGDKeyLength)
            {
                args.GGDKey = _lLuhnModNGenerator.CalculateCheckCode(args.GGDKey);
                return true;
            }

            // Else the code should be 7 digits and validated.
            return args.GGDKey.Length == ValidGGDKeyLength && _luhnModNValidator.Validate(args.GGDKey);
        }
    }
}
 