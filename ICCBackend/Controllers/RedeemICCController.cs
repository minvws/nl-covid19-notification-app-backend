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

namespace NL.Rijksoverheid.ExposureNotification.ICCBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RedeemICCController : ControllerBase
    {
        private readonly ILogger<RedeemICCController> _logger;
        private readonly ActionExecutedContext _Context;
        private readonly ICCBackendContentDbContext _DbContext;

        public RedeemICCController( ICCBackendContentDbContext dbContext, ILogger<RedeemICCController> logger)
        {
         
            _DbContext = dbContext;
            _logger = logger;
        }

        [HttpPost, Authorize]
        public async Task<ActionResult<object>> PostRedeemICC(object payload)
        {
            // POST /labresult call on App Backend
            
            // Delete ICC in DB regardless of prev. step results

            // return valid response

            // Make ICC Used, so it can only be used once 
            InfectionConfirmationCodeEntity ICC =
                await _DbContext.InfectionConfirmationCodes.FindAsync(User.Identity.Name);
            ICC.Used = DateTime.Now;
            await _DbContext.SaveChangesAsync();
            
            return new JsonResult(new
            {
                ok = true,
                status = 501,
                auth=User.Identity.Name,
                payload = payload
            });

            // return Unauthorized(new {ok = false, status = "401", payload = payload});
        }
    }
}