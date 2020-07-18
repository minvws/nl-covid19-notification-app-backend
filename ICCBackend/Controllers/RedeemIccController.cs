// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.Models;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.Services;
using NL.Rijksoverheid.ExposureNotification.IccBackend.Services;

namespace NL.Rijksoverheid.ExposureNotification.IccBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RedeemIccController : ControllerBase
    {
        private readonly ILogger _Logger;
        private readonly IIccService _IccService;
        private readonly AppBackendService _AppBackendService;
        private readonly IccBackendContentDbContext _DbContext;

        public RedeemIccController(ILogger<RedeemIccController> logger, IIccService iccService, AppBackendService appBackendService, IccBackendContentDbContext dbContext)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _IccService = iccService ?? throw new ArgumentNullException(nameof(iccService));
            _AppBackendService = appBackendService ?? throw new ArgumentNullException(nameof(appBackendService));
            _DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        [HttpPost, Authorize]
        public async Task<ActionResult<object>> PostRedeemIcc(ConfirmLabConfirmationIdModel confirmLabConfirmationIdModel)
        {
            _Logger.LogInformation("POST RedeemIcc triggered.");
            // Make Icc Used, so it can only be used once 
            _DbContext.SaveAndCommit();

            // POST /labresult call on App Backend
            var isValid = await _AppBackendService.LabConfirmationIdIsValid(confirmLabConfirmationIdModel);

            if (isValid)
            {
                return new JsonResult(new
                {
                    ok = true,
                    status = 200
                });
            }

            //TODO style - guard against bad outcomes with if()...
            return BadRequest(new
                {ok = false, status = "400", message = "Invalid LabConfirmationId", payload = confirmLabConfirmationIdModel});
        }
    }
}