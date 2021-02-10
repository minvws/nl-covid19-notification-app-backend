// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain
{
    public class StandardEksConfig : AppSettingsReader, IEksConfig
    {
        private static readonly ProductionValueDefaultsEksConfig _ProductionValueDefaults = new ProductionValueDefaultsEksConfig();

        public StandardEksConfig(IConfiguration config, string prefix = "ExposureKeySets") : base(config, prefix) { }
        public int TekCountMin => GetConfigValue("TekCount:Min", _ProductionValueDefaults.TekCountMin);
        public int TekCountMax => GetConfigValue("TekCount:Max", _ProductionValueDefaults.TekCountMax); //Low so the file split is tested
        public int PageSize => GetConfigValue(nameof(PageSize), _ProductionValueDefaults.PageSize);
        public bool CleanupDeletesData => GetConfigValue(nameof(CleanupDeletesData), _ProductionValueDefaults.CleanupDeletesData);
        public int LifetimeDays => GetConfigValue(nameof(LifetimeDays), _ProductionValueDefaults.LifetimeDays);
    }
}