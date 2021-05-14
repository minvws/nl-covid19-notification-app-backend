// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.Stuffing
{
    /// <summary>
    /// Based on flat distribution of DSOS and RSN.
    /// </summary>
    public class EksStuffingGeneratorMk2 : IEksStuffingGeneratorMk2
    {
        private readonly ITransmissionRiskLevelCalculationMk2 _TrlCalculation;
        private readonly IRandomNumberGenerator _Rng;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly IEksConfig _EksConfig;

        public EksStuffingGeneratorMk2(ITransmissionRiskLevelCalculationMk2 trlCalculation, IRandomNumberGenerator rng, IUtcDateTimeProvider dateTimeProvider, IEksConfig eksConfig)
        {
            _TrlCalculation = trlCalculation ?? throw new ArgumentNullException(nameof(trlCalculation));
            _Rng = rng ?? throw new ArgumentNullException(nameof(rng));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _EksConfig = eksConfig ?? throw new ArgumentNullException(nameof(eksConfig));
        }

        /// <summary>
        /// NB this still creates EksCreateJobInputEntity in order to create the EKS but they MUST be written to the DK table before the EKS Engine completes.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public EksCreateJobInputEntity[] Execute(int count)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count), "Count > 0.");

            var result = new EksCreateJobInputEntity[count];
            for (var i = 0; i < count; i++)
            {
                var rsn = _DateTimeProvider.Snapshot.Date.AddDays(-_Rng.Next(0, _EksConfig.LifetimeDays)).ToRollingStartNumber();
                var dsos = _Rng.Next(_TrlCalculation.SignificantDayRange.Lo, _TrlCalculation.SignificantDayRange.Hi);
                var reportType = 1;
                var symptomatic = (InfectiousPeriodType)_Rng.Next(0, 1);
                result[i] = new EksCreateJobInputEntity
                {
                    KeyData = _Rng.NextByteArray(UniversalConstants.DailyKeyDataByteCount),
                    RollingPeriod = UniversalConstants.RollingPeriodRange.Hi,
                    RollingStartNumber = rsn,
                    DaysSinceSymptomsOnset = dsos,
                    TransmissionRiskLevel = _TrlCalculation.Calculate(dsos),
                    Symptomatic = dsos < 0 ? InfectiousPeriodType.Symptomatic : symptomatic, // If dsos < 0 then always symptomatic. Else add the random value
                    ReportType = reportType
                };
            }

            return result;
        }
    }
}