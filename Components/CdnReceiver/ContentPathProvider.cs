// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace CdnDataReceiver.Controllers
{
    public class ContentPathProvider : AppSettingsReader, IContentPathProvider
    {
        public ContentPathProvider(IConfiguration config, string? prefix = null) : base(config, prefix)
        {
        }

        public string Manifest => GetValue(nameof(Manifest), "/vws/v01");
        public string AppConfig => GetValue(nameof(AppConfig), "/vws/v01/appconfig");
        public string ResourceBundle => GetValue(nameof(ResourceBundle), "/vws/v01/resourcebundle");
        public string RiskCalculationParameters => GetValue(nameof(RiskCalculationParameters), "/vws/v01/riskcalculationparameters");
        public string ExposureKeySet => GetValue(nameof(ExposureKeySet), "/vws/v01/exposurekeyset");
    }
}