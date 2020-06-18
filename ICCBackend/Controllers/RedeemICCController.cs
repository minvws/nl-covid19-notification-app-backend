using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc;
using NL.Rijksoverheid.ExposureNotification.IccBackend.Models;

namespace NL.Rijksoverheid.ExposureNotification.IccBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RedeemIccController : ControllerBase
    {
        private readonly ILogger<RedeemIccController> _logger;
        private readonly ActionExecutedContext _Context;
        private readonly IIccService _IccService;
        private readonly AppBackendService _AppBackendService;

        public RedeemIccController(IIccService iccService, ILogger<RedeemIccController> logger, AppBackendService appBackendService)
        {
            _IccService = iccService;
            _AppBackendService = appBackendService;
            _logger = logger;
        }

        [HttpPost, Authorize]
        public async Task<ActionResult<object>> PostRedeemIcc(RedeemIccModel redeemIccModel)
        {
            // Make Icc Used, so it can only be used once 
            InfectionConfirmationCodeEntity Icc = await _IccService.RedeemIcc(User.Identity.Name);
            
            // POST /labresult call on App Backend
            bool LabCID_IsValid = await _AppBackendService.LabConfirmationIDIsValid(redeemIccModel);
            if (LabCID_IsValid)
            {
                return new JsonResult(new
                {
                    ok = true,
                    status = 501,
                    Icc = Icc,
                    payload = redeemIccModel
                });
            }
            return Unauthorized(new {ok = false, status = "401", payload = redeemIccModel});
        }
    }
}