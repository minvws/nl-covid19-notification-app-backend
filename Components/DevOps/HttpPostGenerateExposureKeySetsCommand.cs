// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.ContentFormatters;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps
{
    public class HttpPostGenerateExposureKeySetsCommand
    {
        private readonly WorkflowDbContext _Input;
        private readonly ExposureContentDbContext _Output;
        private readonly IUtcDateTimeProvider _UtcDateTimeProvider;
        private readonly IEfDbConfig _StandardEfDbConfig; //Job!
        private readonly IGaenContentConfig _GaenContentConfig;
        private readonly IExposureKeySetHeaderInfoConfig _HsmExposureKeySetHeaderInfoConfig;
        private readonly IExposureKeySetBatchJobConfig _ExposureKeySetBatchJobConfig;

        public HttpPostGenerateExposureKeySetsCommand(WorkflowDbContext input, ExposureContentDbContext output, IUtcDateTimeProvider utcDateTimeProvider, IEfDbConfig standardEfDbConfig, IGaenContentConfig gaenContentConfig, IExposureKeySetHeaderInfoConfig hsmExposureKeySetHeaderInfoConfig, IExposureKeySetBatchJobConfig exposureKeySetBatchJobConfig)
        {
            _Input = input;
            _Output = output;
            _UtcDateTimeProvider = utcDateTimeProvider;
            _StandardEfDbConfig = standardEfDbConfig;
            _GaenContentConfig = gaenContentConfig;
            _HsmExposureKeySetHeaderInfoConfig = hsmExposureKeySetHeaderInfoConfig;
            _ExposureKeySetBatchJobConfig = exposureKeySetBatchJobConfig;
        }

        public async Task<IActionResult> Execute()
        {
                var jobConfigBuilder = new SqlServerDbContextOptionsBuilder(_StandardEfDbConfig);

                using var bb = new ExposureKeySetBatchJobMk2(
                    new DbTekSource(_Input),
                    jobConfigBuilder,
                    _UtcDateTimeProvider,
                    new ExposureKeySetDbWriter(_Output, new Sha256PublishingId(new HardCodedExposureKeySetSigning())),
                    _GaenContentConfig, 
                    //new JsonContentExposureKeySetFormatter(),
                    new ExposureKeySetBuilderV1(
                        _HsmExposureKeySetHeaderInfoConfig,
                        new HardCodedExposureKeySetSigning(), _UtcDateTimeProvider, new GeneratedProtobufContentFormatter())
                    , _ExposureKeySetBatchJobConfig
                );

                await bb.Execute();

                return new OkResult();
        }   
    }
}
