// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RivmAdvice;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest
{
    public class ManifestBuilder
    {
        private readonly GetActiveExposureKeySetsListCommand _ExposureKeySetsListCommand;
        private readonly GetLatestRivmAdviceCommand _RivmAdviceFinder;
        private readonly GetLatestRiskCalculationConfigCommand _WorkflowCalcConfigFinder;

        public ManifestBuilder(GetActiveExposureKeySetsListCommand ExposureKeySetsListCommand, GetLatestRivmAdviceCommand rivmAdviceFinder, GetLatestRiskCalculationConfigCommand WorkflowCalcConfigFinder)
        {
            _ExposureKeySetsListCommand = ExposureKeySetsListCommand;
            _RivmAdviceFinder = rivmAdviceFinder;
            _WorkflowCalcConfigFinder = WorkflowCalcConfigFinder;
        }

        public ManifestResponse Execute()
        {
            return new ManifestResponse
            { 
                ExposureKeySets = _ExposureKeySetsListCommand.Execute(),
                RivmAdvice = _RivmAdviceFinder.Execute(),
                RiskCalculationConfig = _WorkflowCalcConfigFinder.Execute(),
            };
        }
    }
}