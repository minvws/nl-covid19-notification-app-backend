// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain
{
    public class ProductionValueDefaultsEksConfig : IEksConfig
    {
        public int TekCountMin => 150;
        public int TekCountMax => 150000;

        //There is no value set for this in deployment pipeline.
        public int PageSize => 10000;

        public bool CleanupDeletesData => false;
        public int LifetimeDays => 14;
    }
}
