// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.ContentFormatters;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DataUtilities.Components;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DataUtilities.GenExposureKeySets
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var standardUtcDateTimeProvider = new StandardUtcDateTimeProvider();
                var config = AppSettingsFromJsonFiles.GetConfigurationRoot();

                using var inputContext = new DbContextProvider<WorkflowDbContext>(
                    () => new WorkflowDbContext(new PostGresDbContextOptionsBuilder(new StandardEfDbConfig(config, "Input")).Build())
                );

                var jobConfigBuilder = new PostGresDbContextOptionsBuilder(new StandardEfDbConfig(config, "Job"));
                using var jobContext = new DbContextProvider<ExposureKeySetsBatchJobDbContext>(
                    () => new ExposureKeySetsBatchJobDbContext(jobConfigBuilder.Build())
                );
                
                using var outputContext = new DbContextProvider<ExposureContentDbContext>(
                    () => new ExposureContentDbContext(new PostGresDbContextOptionsBuilder(new StandardEfDbConfig(config, "Output")).Build())
                );

                using var bb = new ExposureKeySetBatchJob(
                    new DbTekSource(inputContext),
                    jobConfigBuilder,
                    standardUtcDateTimeProvider,
                    new ExposureKeySetDbWriter(outputContext, new Sha256PublishingIdCreator(new HardCodedExposureKeySetSigning())),
                    new HardCodedAgConfig(),
                    new JsonContentExposureKeySetFormatter(), 
                    new ExposureKeySetBuilderV1(
                        new HsmExposureKeySetHeaderInfoConfig(config),
                        new HardCodedExposureKeySetSigning(), standardUtcDateTimeProvider, new GeneratedProtobufContentFormatter())
                    , new ExposureKeySetBatchJobConfig(config)
                );

                bb.Execute().GetAwaiter().GetResult();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}
