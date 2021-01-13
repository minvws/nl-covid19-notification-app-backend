using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.IccPortal.Controllers
{
    public class AccountController : Controller
    {
        [AllowAnonymous, HttpGet]
        public IActionResult AccessDenied([FromServices] HttpGetLogoutCommand logoutCommand, [FromServices] HttpGetAccessDeniedCommand accessDeniedCommand)
        {
            logoutCommand.ExecuteAsync(HttpContext); // logs out without using the redirectresult 
            return accessDeniedCommand.Execute(HttpContext);
        }
    }
}
