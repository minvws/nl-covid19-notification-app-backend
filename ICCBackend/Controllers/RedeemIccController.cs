// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ICC.Models;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ICC.Services;
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
        private readonly IccBackendContentDbContext _DbContext;

        public RedeemIccController(IIccService iccService, ILogger<RedeemIccController> logger,
            AppBackendService appBackendService, IccBackendContentDbContext dbContext)
        {
            _IccService = iccService;
            _AppBackendService = appBackendService;
            _Logger = logger;
            _DbContext = dbContext;
        }

        [HttpPost, Authorize]
        public async Task<ActionResult<object>> PostRedeemIcc(ConfirmLabConfirmationIdModel confirmLabConfirmationIdModel)
        {
            // Make Icc Used, so it can only be used once 
            var infectionConfirmationCodeEntity = await _IccService.RedeemIcc(User.Identity.Name);
            _DbContext.SaveAndCommit();

            // POST /labresult call on App Backend
            bool isValid = false;
            try
            {
                isValid = await _AppBackendService.LabConfirmationIdIsValid(confirmLabConfirmationIdModel);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            if (isValid)
            {
                return new JsonResult(new
                {
                    ok = true,
                    status = 200
                });
            }

            return BadRequest(new
                {ok = false, status = "400", message = "Invalid LabConfirmationId", payload = confirmLabConfirmationIdModel});
        }
        

    }
}