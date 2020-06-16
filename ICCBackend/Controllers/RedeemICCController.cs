using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.ICCBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RedeemICCController : ControllerBase
    {
        private readonly ILogger<RedeemICCController> _logger;

        public RedeemICCController(ILogger<RedeemICCController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<object>> PostRedeemICC(object payload)
        {
            // Check if ICC from authorization header exists in ICC DB
            // POST /labresult call on App Backend


            // Delete ICC in DB regardless of prev. step results

            // return valid response
            // if ((new String(payload.GetType().GetProperty("icc").GetValue(payload,null).ToString())).Length < 32)
            // {
            //     return new JsonResult( new {
            //         ok = true,
            //         status = 400,
            //         message = "icc_length",
            //         payload = payload
            //     });
            // }
                return new JsonResult(new
                {
                    ok = true,
                    status = 501,
                    payload = payload
                });

            // return Unauthorized(new {ok = false, status = "401", payload = payload});
        }
    }
}