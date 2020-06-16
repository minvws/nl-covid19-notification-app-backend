using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.Authorisation;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.KeyReleasePortalApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthorisationController : ControllerBase
    {
        [HttpPost]
        [Route(EndPointNames.CaregiversPortalApi.KeysLastWorkflow.LabConfirmation)]
        public async Task<IActionResult> PostAuthorise([FromBody]KeysLastAuthorisationArgs args, [FromServices]HttpPostKeysLastAuthorise command)
        {
            return await command.Execute(args);
        }
    }
}
