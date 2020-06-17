using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EKSEngineApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthorisationController : ControllerBase
    {
        /// <summary>
        /// Generate new ExposureKeySets.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route(EndPointNames.DevOps.ExposureKeySetsCreate)]
        public async Task<IActionResult> ExposureKeySets([FromServices]HttpPostGenerateExposureKeySetsCommand command)
        {
            return await command.Execute();
        }
    }
}
