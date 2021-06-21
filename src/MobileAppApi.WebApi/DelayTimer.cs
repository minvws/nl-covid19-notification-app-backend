// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi
{
    public static class DelayTimer
    {
        public static async Task Delay(int milliseconds)
        {
            var timer = new Stopwatch();

            timer.Start();
            while (timer.ElapsedMilliseconds <= milliseconds)
            {
                // just sit and wait
            }

            timer.Stop();
        }
    }
}
