using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ICC;
using NL.Rijksoverheid.ExposureNotification.ICCBackend.Models;

namespace NL.Rijksoverheid.ExposureNotification.ICCBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RedeemICCController : ControllerBase
    {
        private readonly ILogger<RedeemICCController> _logger;
        private readonly ActionExecutedContext _Context;
        private readonly IICCService _ICCService;
        private readonly AppBackendService _AppBackendService;

        public RedeemICCController(IICCService iccService, ILogger<RedeemICCController> logger, AppBackendService appBackendService)
        {
            _ICCService = iccService;
            _AppBackendService = appBackendService;
            _logger = logger;
        }

        [HttpPost, Authorize]
        public async Task<ActionResult<object>> PostRedeemICC(RedeemICCModel redeemIccModel)
        {
            // Make ICC Used, so it can only be used once 
            InfectionConfirmationCodeEntity ICC = await _ICCService.RedeemICC(User.Identity.Name);
            
            // POST /labresult call on App Backend
            bool LabCID_IsValid = await _AppBackendService.LabConfirmationIDIsValid(redeemIccModel);
            if (LabCID_IsValid)
            {
                return new JsonResult(new
                {
                    ok = true,
                    status = 501,
                    ICC = ICC,
                    payload = redeemIccModel
                });
            }
            return Unauthorized(new {ok = false, status = "401", payload = redeemIccModel});
        }
    }
}