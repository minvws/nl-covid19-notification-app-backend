// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AppConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest
{
    public class ManifestBuilder
    {
        private readonly GetActiveExposureKeySetsListCommand _ExposureKeySetsListCommand;
        private readonly GetLatestContentCommand<RiskCalculationContentEntity> _WorkflowCalcParametersFinder;
        private readonly GetLatestContentCommand<AppConfigContentEntity> _AppConfigFinder;

        public ManifestBuilder(GetActiveExposureKeySetsListCommand exposureKeySetsListCommand, GetLatestContentCommand<RiskCalculationContentEntity> workflowCalcParametersFinder, GetLatestContentCommand<AppConfigContentEntity> appConfigFinder)
        {
            _ExposureKeySetsListCommand = exposureKeySetsListCommand ?? throw new ArgumentNullException(nameof(exposureKeySetsListCommand));
            _WorkflowCalcParametersFinder = workflowCalcParametersFinder ?? throw new ArgumentNullException(nameof(workflowCalcParametersFinder));
            _AppConfigFinder = appConfigFinder ?? throw new ArgumentNullException(nameof(appConfigFinder));
        }

        public ManifestContent Execute()
        {
            return new ManifestContent
            { 
                ExposureKeySets = _ExposureKeySetsListCommand.Execute(),
                RiskCalculationParameters = _WorkflowCalcParametersFinder.Execute(),
                AppConfig = _AppConfigFinder.Execute()
            };
        }
    }
}