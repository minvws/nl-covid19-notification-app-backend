// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DataUtilities.Components;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DataUtilities.GenWorkflows
{

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var config = AppSettingsFromJsonFiles.GetConfigurationRoot();
                using var dbContextProvider = new DbContextProvider<ExposureContentDbContext>(
                    () => new ExposureContentDbContext(new PostGresDbContextOptionsBuilder(new StandardEfDbConfig(config, "Content")).Build())
                );


                var WorkflowCount = Convert.ToInt32(args[0]);
                if (WorkflowCount < 1)
                    throw new ArgumentOutOfRangeException(nameof(WorkflowCount));
                
                var randomSeed = Convert.ToInt32(args[1]);
                
                var r = new Random(randomSeed); //Don't need the crypto version.
                var c1 = new GenerateAgWorkflows(dbContextProvider);
                c1.Execute(WorkflowCount, x => r.Next(x), x => r.NextBytes(x));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }
    }
}
