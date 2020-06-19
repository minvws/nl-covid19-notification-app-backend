// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.Models;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ICC.Models;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ICC.Services;
using NL.Rijksoverheid.ExposureNotification.IccBackend.Services;

namespace NL.Rijksoverheid.ExposureNotification.IccBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RevokeBatchController : ControllerBase
    {
        private readonly ILogger<RevokeBatchController> _Logger;
        private readonly ActionExecutedContext _Context;
        private readonly IIccService _IccService;
        private readonly AppBackendService _AppBackendService;
        private readonly IccBackendContentDbContext _DbContext;

        public RevokeBatchController(IIccService iccService, ILogger<RevokeBatchController> logger,
            AppBackendService appBackendService, IccBackendContentDbContext dbContext)
        {
            _IccService = iccService;
            _AppBackendService = appBackendService;
            _Logger = logger;
            _DbContext = dbContext;
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> PostRevokeBatch([FromBody] RevokeBatchInput revokeBatchInput)
        {
            var infectionConfirmationCodeEntity = await _IccService.RedeemIcc(User.Identity.Name);

            bool result = await _IccService.RevokeBatch(revokeBatchInput);

            _DbContext.SaveAndCommit();

            if (result) return new OkResult();
            return NotFound(new
            {
                ok = false,
                status = 404,
                message = "Batch not found."
            });
        }
    }
}