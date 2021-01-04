// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound
{
    public class IksConfig : AppSettingsReader, IIksConfig
    {
        public IksConfig(IConfiguration config, string? prefix = null) : base(config, prefix)
        {
        }

        public int ItemCountMax => GetConfigValue(nameof(ItemCountMax), 750000); 
        public int PageSize => GetConfigValue(nameof(PageSize), 10000);
    }
}