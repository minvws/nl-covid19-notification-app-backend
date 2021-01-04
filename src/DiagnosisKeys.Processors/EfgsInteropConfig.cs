// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors
{
    
    /// <summary>
    /// Do not use in production.
    /// Create specific classes for the required settings but ensure and invalid value is set for the default.
    /// </summary>
    public class EfgsInteropConfig : AppSettingsReader, IAcceptableCountriesSetting, IOutboundFixedCountriesOfInterestSetting, IEksEngineConfig
    {
        private readonly CountryCodeListParser _CountryCodeListParser = new CountryCodeListParser();

        //TODO organise settings properly
        public EfgsInteropConfig(IConfiguration config, string? prefix = "Interop:Temp") : base(config, prefix)
        {
        }

        private const string DefaultCountryList = "BE,GR,LT,PT,BG,ES,LU,RO,CZ,FR,HU,SI,DK,HR,MT,SK,DE,IT,NL,FI,EE,CY,AT,SE,IE,LV,PL,IS,NO,LI,CH";

        /// <summary>
        /// Inbound setting
        /// </summary>
        public string[] AcceptableCountries => _CountryCodeListParser.Parse(GetConfigValue(nameof(AcceptableCountries), DefaultCountryList));

        /// <summary>
        /// Outbound setting
        /// </summary>
        public string[] CountriesOfInterest => _CountryCodeListParser.Parse(GetConfigValue(nameof(CountriesOfInterest), DefaultCountryList));

        public bool IksImportEnabled => GetConfigValue(nameof(IksImportEnabled), true);
    }
}
