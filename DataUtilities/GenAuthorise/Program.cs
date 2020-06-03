// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.KeysFirstWorkflow.WorkflowAuthorisation;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DataUtilities.Components;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DataUtilities.Authorise
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var pAuthorise = Convert.ToInt32(args[0]);

                if (0 >= pAuthorise || pAuthorise > 100)
                    throw new ArgumentException(nameof(pAuthorise));

                var seed = Convert.ToInt32(args[1]);

                var config = AppSettingsFromJsonFiles.GetConfigurationRoot();
                using var dbContextProvider = new DbContextProvider<ExposureContentDbContext>(
                    () => new ExposureContentDbContext(new PostGresDbContextOptionsBuilder(new StandardEfDbConfig(config, "MSS")).Build())
                    );

                var authorise = new GenerateAuthorisations(dbContextProvider, new WorkflowDbAuthoriseCommand(dbContextProvider));
                authorise.Execute(pAuthorise, new Random(seed));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }
    }
}
