// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Security.Principal;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Framework
{
    public static class WindowsIdentityStuff
    {
        public static bool CurrentUserIsAdministrator()
        {
            return false;
        }
    }
}
