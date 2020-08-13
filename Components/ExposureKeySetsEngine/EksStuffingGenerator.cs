// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine
{
    public class EksStuffingGenerator : IEksStuffingGenerator
    {
        private readonly IRandomNumberGenerator _Random;
        private readonly ITekValidatorConfig _TekValidatorConfig;
        
        private StuffingArgs _Args;

        public EksStuffingGenerator(IRandomNumberGenerator random, ITekValidatorConfig tekValidatorConfig)
        {
            _Random = random ?? throw new ArgumentNullException(nameof(random));
            _TekValidatorConfig = tekValidatorConfig ?? throw new ArgumentNullException(nameof(tekValidatorConfig));
        }

        public EksCreateJobInputEntity[] Execute(StuffingArgs args)
        {
            _Args = args ?? throw new ArgumentNullException();
            if (args.Count < 1)
                throw new ArgumentOutOfRangeException(nameof(args), "Count < 1");

            var result = new EksCreateJobInputEntity[args.Count];
            for (var i = 0; i < args.Count; i++)
            {
                result[i] = new EksCreateJobInputEntity
                {
                    RollingPeriod = _TekValidatorConfig.RollingPeriodMax, //Could randomise - would need > 1 Tek for current date to look legit.
                    RollingStartNumber = GetRandomRollingStartNumber(),
                    KeyData = _Random.NextByteArray(_TekValidatorConfig.KeyDataLength),
                    TransmissionRiskLevel = TransmissionRiskLevel.Low
                };
            }

            return result;
        }
        
        //Which day?
        private int GetRandomRollingStartNumber()
        {
            var delta = _Random.Next(0, _TekValidatorConfig.MaxAgeDays);
            var date = _Args.JobTime.Date.AddDays(-delta);
            return date.ToRollingStartNumber();
        }
    }
}