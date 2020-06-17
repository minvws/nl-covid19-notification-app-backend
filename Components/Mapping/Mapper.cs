// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AppConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.EscrowTeks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow;
using TemporaryExposureKeyArgs = NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.TemporaryExposureKeyArgs;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping
{
    public static class Mapper
    {
        public static RiskCalculationConfigContent ToContent(this RiskCalculationConfigArgs args)
            => new RiskCalculationConfigContent
            {
                MinimumRiskScore = args.MinimumRiskScore,
                DaysSinceLastExposureScores​ = args.DaysSinceLastExposureScores​,
                AttenuationScores​ = args.AttenuationScores​,
                DurationAtAttenuationThresholds​ = args.DurationAtAttenuationThresholds​,
                DurationScores = args.DurationScores,
                TransmissionRiskScores​ = args.TransmissionRiskScores​
            };

        public static TemporaryExposureKeyEntity[] ToEntities(this TemporaryExposureKeyArgs[] items)
        {
            var content = items.Select(x =>
                new TemporaryExposureKeyEntity
                {
                    KeyData = Convert.FromBase64String(x.KeyData),
                    TransmissionRiskLevel = x.TransmissionRiskLevel,
                    RollingPeriod = x.RollingPeriod,
                    RollingStartNumber = x.RollingStartNumber,
                }).ToArray();

            return content;
        }

        public static ResourceBundleContent ToContent(this ResourceBundleArgs args)
            => new ResourceBundleContent
            {
                Text = args.Text,
                IsolationPeriodDays = args.IsolationPeriodDays,
                ObservedTemporaryExposureKeyRetentionDays = args.ObservedTemporaryExposureKeyRetentionDays,
                TemporaryExposureKeyRetentionDays = args.TemporaryExposureKeyRetentionDays,
            };

        public static AppConfigContent ToContent(this AppConfigArgs args)
            => new AppConfigContent
            {
                DecoyProbability = args.DecoyProbability,
                ManifestFrequency = args.ManifestFrequency,
                Version = args.Version
            };

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