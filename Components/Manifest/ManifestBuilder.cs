// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest
{
    public class ManifestBuilder
    {
        private readonly GetActiveExposureKeySetsListCommand _ExposureKeySetsListCommand;
        private readonly GetLatestResourceBundleCommand _ResourceBundleFinder;
        private readonly GetLatestRiskCalculationParametersCommand _WorkflowCalcParametersFinder;

        public ManifestBuilder(GetActiveExposureKeySetsListCommand exposureKeySetsListCommand, GetLatestResourceBundleCommand resourceBundleFinder, GetLatestRiskCalculationParametersCommand workflowCalcParametersFinder)
        {
            _ExposureKeySetsListCommand = exposureKeySetsListCommand;
            _ResourceBundleFinder = resourceBundleFinder;
            _WorkflowCalcParametersFinder = workflowCalcParametersFinder;
        }

        public ManifestResponse Execute()
        {
            return new ManifestResponse
            { 
                ExposureKeySets = _ExposureKeySetsListCommand.Execute(),
                ResourceBundle = _ResourceBundleFinder.Execute(),
                RiskCalculationParameters = _WorkflowCalcParametersFinder.Execute(),
            };
        }
    }
}