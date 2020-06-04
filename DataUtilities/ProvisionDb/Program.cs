// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DataUtilities.Components;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DataUtilities.ProvisionDb
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var config = AppSettingsFromJsonFiles.GetConfigurationRoot();
                using var contentDb = new DbContextProvider<ExposureContentDbContext>(
                    () => new ExposureContentDbContext(new PostGresDbContextOptionsBuilder(new StandardEfDbConfig(config, "Content")).Build())
                    //() => new ExposureContentDbContext(new SqlServerDbContextOptionsBuilder(new StandardEfDbConfig(config, "Content")).Build())
                );

                var dpProvision = new CreateDatabaseAndCollections(contentDb, new Sha256PublishingIdCreator(new HardCodedExposureKeySetSigning()));
                dpProvision.Execute().GetAwaiter().GetResult();

                using var workflowDb = new DbContextProvider<WorkflowDbContext>(
                    () => new WorkflowDbContext(new PostGresDbContextOptionsBuilder(new StandardEfDbConfig(config, "Workflow")).Build())
                    //() => new ExposureContentDbContext(new SqlServerDbContextOptionsBuilder(new StandardEfDbConfig(config, "Content")).Build())
                );

                workflowDb.Current.Database.EnsureCreated();

                Console.WriteLine("Completed.");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }
    }


}
