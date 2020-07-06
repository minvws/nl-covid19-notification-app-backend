// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AppConfig
{
    public class AppConfigValidator
    {
        public bool Valid(AppConfigArgs args)
        {
            if (args == null)
                return false;

            if (args.ManifestFrequency < 1)
                return false;

            if (args.DecoyProbability < 1)
                return false;

            if (args.Version < 1) //todo Need to be highest version in database or not??
                return false;

            return true;
        }
    }
}
