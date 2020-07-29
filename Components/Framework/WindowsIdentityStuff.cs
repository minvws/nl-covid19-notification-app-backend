using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Framework
{
    public static class WindowsIdentityStuff
    {
        public static bool CurrentUserIsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
