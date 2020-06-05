// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AgProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.ContentFormatters;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps.Commands
{
    public class HttpPostGenerateExposureKeySetsCommand
    {
        private readonly IConfiguration _config;
        private readonly IDbContextProvider<WorkflowDbContext> _input;
        private readonly IDbContextProvider<ExposureContentDbContext> _output;
        private readonly IDbContextProvider<ExposureKeySetsBatchJobDbContext> _jobs;
        private readonly IUtcDateTimeProvider _standardUtcDateTimeProvider;

        public HttpPostGenerateExposureKeySetsCommand(IConfiguration config, IDbContextProvider<WorkflowDbContext> input, IDbContextProvider<ExposureContentDbContext> output, IDbContextProvider<ExposureKeySetsBatchJobDbContext> jobs, IUtcDateTimeProvider standardUtcDateTimeProvider)
        {
            _config = config;
            _input = input;
            _output = output;
            _jobs = jobs;
            _standardUtcDateTimeProvider = standardUtcDateTimeProvider;
        }

        public async Task<IActionResult> Execute()
        {
            try
            {
                using var bb = new ExposureKeySetBatchJob(
                    new DbTekSource(_input),
                    _jobs,
                    _standardUtcDateTimeProvider,
                    new ExposureKeySetDbWriter(_output, new Sha256PublishingIdCreator(new HardCodedExposureKeySetSigning())),
                    new AgConfigAppSettings(_config), 
                    new JsonContentExposureKeySetFormatter(), 
                    new ExposureKeySetBuilderV1(
                        new HsmExposureKeySetHeaderInfoConfig(_config),
                        new HardCodedExposureKeySetSigning(), _standardUtcDateTimeProvider, new GeneratedProtobufContentFormatter())
                    , new ExposureKeySetBatchJobConfig(_config)
                );

                await bb.Execute();

                return new OkResult();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return new BadRequestResult();
            }
        }   
    }
}
