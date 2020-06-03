// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DataUtilities.Components
{
    public class GenerateAgWorkflows
    {
        private readonly IDbContextProvider<ExposureContentDbContext>_DbContextProvider;

        public GenerateAgWorkflows(IDbContextProvider<ExposureContentDbContext>dbContextProvider)
        {
            _DbContextProvider = dbContextProvider;
        }

        public void Execute(int WorkflowCount, Func<int, int> randomInt, Action<byte[]> randomBytes)
        {
            var luhnModNConfig = new LuhnModNConfig();
            var WorkflowKeyGenerator = new GenerateWorkflowKeys(luhnModNConfig);
            var WorkflowValidatorConfig = new HardCodedAgWorkflowValidatorConfig();
            var WorkflowKeyValidatorConfig = new HardCodedAgTemporaryExposureKeyValidatorConfig();
            var writer = new WorkflowInsertDbCommand(_DbContextProvider, new StandardUtcDateTimeProvider());
            var c = new HttpPostWorkflowCommand(
                writer,
                new WorkflowValidator(
                    WorkflowValidatorConfig,
                    new WorkflowAuthorisationTokenLuhnModNValidator(luhnModNConfig),
                        new TemporaryExposureKeyValidator(WorkflowKeyValidatorConfig)), 
                _DbContextProvider);

            var keyBuffer = new byte[WorkflowKeyValidatorConfig.DailyKeyByteCount];

            for (var i = 0; i < WorkflowCount; i++)
            {
                var Workflow = new WorkflowArgs
                {
                    Token = WorkflowKeyGenerator.Next(randomInt)
                };

                var keyCount = 1 + randomInt(WorkflowValidatorConfig.WorkflowKeyCountMax - 1);
                var keys = new List<WorkflowKeyArgs>(keyCount);
                for (var j = 0; j < keyCount; j++)
                {
                    randomBytes(keyBuffer);
                    keys.Add(new WorkflowKeyArgs
                    {
                        KeyData = Convert.ToBase64String(keyBuffer),
                        RollingStartNumber = WorkflowKeyValidatorConfig.RollingPeriodMin + j,
                        RollingPeriod = 11,
                        TransmissionRiskLevel = 2
                    });
                }
                Workflow.Items = keys.ToArray();

                c.Execute(Workflow);

                if (i > 0 && i % 100 == 0)
                    Console.WriteLine($"Workflow count {i}...");
            }
            Console.WriteLine("Generating Workflows finished.");
        }
    }
}
