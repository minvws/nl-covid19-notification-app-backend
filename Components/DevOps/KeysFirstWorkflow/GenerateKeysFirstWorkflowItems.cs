// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.EscrowTeks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps.KeysFirstWorkflow
{
    public class GenerateKeysFirstWorkflowItems
    {
        private readonly WorkflowDbContext _DbContextProvider;

        public GenerateKeysFirstWorkflowItems(WorkflowDbContext dbContextProvider)
        {
            _DbContextProvider = dbContextProvider;
        }

        public async Task Execute(int itemCount, Func<int, int> randomInt, Action<byte[]> randomBytes)
        {
            var luhnModNConfig = new LuhnModNConfig();
            var tokenGenerator = new GenerateKeysFirstAuthorisationToken(luhnModNConfig);
            var workflowValidationConfig = new DefaultGeanTekListValidationConfig();
            var tekValidatorConfig = new DefaultGaenTekValidatorConfig();
            var writer = new KeysFirstEscrowInsertDbCommand(_DbContextProvider, new StandardUtcDateTimeProvider());
            var c = new HttpPostKeysFirstEscrowCommand(
                writer,
                new KeysFirstEscrowValidator(
                    workflowValidationConfig,
                    new KeysFirstAuthorisationTokenLuhnModNValidator(luhnModNConfig),
                        new TemporaryExposureKeyValidator(tekValidatorConfig)), 
                _DbContextProvider);

            var keyBuffer = new byte[tekValidatorConfig.DailyKeyByteCount];

            for (var i = 0; i < itemCount; i++)
            {
                var workflow = new KeysFirstEscrowArgs
                {
                    Token = tokenGenerator.Next(randomInt)
                };

                var keyCount = 1 + randomInt(workflowValidationConfig.TemporaryExposureKeyCountMax - 1);
                var keys = new List<TemporaryExposureKeyArgs>(keyCount);
                for (var j = 0; j < keyCount; j++)
                {
                    randomBytes(keyBuffer);
                    keys.Add(new TemporaryExposureKeyArgs
                    {
                        KeyData = Convert.ToBase64String(keyBuffer),
                        RollingStartNumber = tekValidatorConfig.RollingPeriodMin + j,
                        RollingPeriod = 11,
                        TransmissionRiskLevel = 2
                    });
                }
                workflow.Items = keys.ToArray();

                await c.Execute(workflow);
            }
        }
    }
}
