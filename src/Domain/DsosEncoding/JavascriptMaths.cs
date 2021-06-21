// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.DsosEncoding
{
    public static class JavascriptMaths
    {
        //JS Math.Round:
        public static int Round(double value)
        {
            return (int)Math.Floor(value + 0.5);
        }
    }
}
