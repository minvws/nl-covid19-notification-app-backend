using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.Models;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc
{
    public class HttpGetUserClaimCommand
    {
        //TODO always 1 user? and 1 claim? Just return a json encoded string?
        public IActionResult Execute(HttpContext httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));

            var claimValue = httpContext.User?.Claims?.FirstOrDefault(c => c.Type.Equals(ClaimTypes.NameIdentifier))?.Value;

            if (string.IsNullOrWhiteSpace(claimValue))
                return new UnauthorizedResult();
            
            return new OkObjectResult(new ClaimInfoResponse { User = new ClaimInfo { Id = claimValue } });
        }
    }
}