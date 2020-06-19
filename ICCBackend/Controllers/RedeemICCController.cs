// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ICC.Models;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ICC.Services;
using NL.Rijksoverheid.ExposureNotification.IccBackend.Models;
using NL.Rijksoverheid.ExposureNotification.IccBackend.Services;

namespace NL.Rijksoverheid.ExposureNotification.IccBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RedeemIccController : ControllerBase
    {
        private readonly ILogger<RedeemIccController> _Logger;
        private readonly ActionExecutedContext _Context;
        private readonly IIccService _IccService;
        private readonly AppBackendService _AppBackendService;

        public RedeemIccController(IIccService iccService, ILogger<RedeemIccController> logger,
            AppBackendService appBackendService)
        {
            _IccService = iccService;
            _AppBackendService = appBackendService;
            _Logger = logger;
        }

        [HttpPost, Authorize]
        public async Task<ActionResult<object>> PostRedeemIcc(RedeemIccModel redeemIccModel)
        {
            // Make Icc Used, so it can only be used once 
            // InfectionConfirmationCodeEntity infectionConfirmationCodeEntity = await _IccService.RedeemIcc(User.Identity.Name);

            // POST /labresult call on App Backend
            bool isValid = await _AppBackendService.LabConfirmationIdIsValid(redeemIccModel);
            // bool isValid = false;
            if (isValid)
            {
                // TODO: remove debugging Icc and payload from response
                return new JsonResult(new
                {
                    ok = true,
                    status = 200,
                    payload = redeemIccModel
                });
            }

            return BadRequest(new
                {ok = false, status = "400", message = "Invalid LabConfirmationId", payload = redeemIccModel});
        }
    }
}