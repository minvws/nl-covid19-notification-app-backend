// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;
using System.Text;
using Newtonsoft.Json;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.EscrowTeks;
using TemporaryExposureKeyArgs = NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.EscrowTeks.TemporaryExposureKeyArgs;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping
{
    public static class Mapper
    {
        public static RiskCalculationConfigResponse ToResponse(this RiskCalculationConfigContent e)
            => new RiskCalculationConfigResponse
            {
                MinimumRiskScore = e.MinimumRiskScore,
                Attenuation = new WeightingResponse {Weight = e.Attenuation.Weight, LevelValues = e.Attenuation.LevelValues},
                DaysSinceLastExposure = new WeightingResponse {Weight = e.DaysSinceLastExposure.Weight, LevelValues = e.DaysSinceLastExposure.LevelValues},
                DurationLevelValues = new WeightingResponse {Weight = e.DurationLevelValues.Weight, LevelValues = e.DurationLevelValues.LevelValues},
                TransmissionRisk = new WeightingResponse {Weight = e.TransmissionRisk.Weight, LevelValues = e.TransmissionRisk.LevelValues},
            };


        public static RiskCalculationContentEntity ToEntity(this RiskCalculationConfigArgs args)
        {
            var content = new RiskCalculationConfigContent
            {
                MinimumRiskScore = args.MinimumRiskScore,
                Attenuation = new WeightingContent {Weight = args.Attenuation.Weight, LevelValues = args.Attenuation.LevelValues},
                DaysSinceLastExposure = new WeightingContent {Weight = args.DaysSinceLastExposure.Weight, LevelValues = args.DaysSinceLastExposure.LevelValues},
                DurationLevelValues = new WeightingContent {Weight = args.DurationLevelValues.Weight, LevelValues = args.DurationLevelValues.LevelValues},
                TransmissionRisk = new WeightingContent {Weight = args.TransmissionRisk.Weight, LevelValues = args.TransmissionRisk.LevelValues},
            };

            return new RiskCalculationContentEntity
            {
                Release = args.Release,
                Content = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(content))
            };
        }

        public static KeysFirstTeksWorkflowEntity ToEntity(this KeysFirstEscrowArgs args)
        {
            var content = args.Items.Select(x =>
                new WorkflowKeyContent
                {
                    KeyData = x.KeyData,
                    TransmissionRiskLevel = x.TransmissionRiskLevel,
                    RollingPeriod = x.RollingPeriod,
                    RollingStartNumber = x.RollingStartNumber
                }).ToArray();

            return new KeysFirstTeksWorkflowEntity
            {
                AuthorisationToken = args.Token,
                TekContent = JsonConvert.SerializeObject(content) //TODO deserialize had better not screw with this cos array...
            };
        }

        public static ResourceBundleContentEntity ToEntity(this ResourceBundleArgs args)
        {
            var content = new ResourceBundleEntityContent
            {
                Text = args.Text,
                IsolationPeriodDays = args.IsolationPeriodDays,
                ObservedTemporaryExposureKeyRetentionDays = args.ObservedTemporaryExposureKeyRetentionDays,
                TemporaryExposureKeyRetentionDays = args.TemporaryExposureKeyRetentionDays,
            };

            return new ResourceBundleContentEntity
            {
                Release = args.Release,
                Content = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(content))
            };
        }

        ///// <summary>
        ///// TODO decide whether we really need to Json version but it is useful to see in a readable form what the content of the zipped AG content is too
        ///// </summary>
        ///// <param name="output"></param>
        ///// <returns></returns>
        //public static ExposureKeySetConfigEntity ToEntity(this ExposureKeySetOutput output)
        //{
        //    var jsonContent = JsonConvert.SerializeObject(output.DebugContentJson);
        //    return new ExposureKeySetConfigEntity
        //    {
        //        Release = output.Created,
        //        CreatingJobQualifier = output.CreatingJobQualifier,
        //        CreatingJobName = output.CreatingJobName,
        //        JsonContent = jsonContent,
        //        AgContent = output.AgContent
        //    };
        //}
    }
}