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
        private readonly ITransmissionRiskLevelCalculationMk2 _trlCalculation;
        private readonly IRandomNumberGenerator _rng;
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly IEksConfig _eksConfig;

        public EksStuffingGeneratorMk2(ITransmissionRiskLevelCalculationMk2 trlCalculation, IRandomNumberGenerator rng, IUtcDateTimeProvider dateTimeProvider, IEksConfig eksConfig)
        {
            _trlCalculation = trlCalculation ?? throw new ArgumentNullException(nameof(trlCalculation));
            _rng = rng ?? throw new ArgumentNullException(nameof(rng));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _eksConfig = eksConfig ?? throw new ArgumentNullException(nameof(eksConfig));
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
                var rsn = _dateTimeProvider.Snapshot.Date.AddDays(-_rng.Next(0, _eksConfig.LifetimeDays)).ToRollingStartNumber();
                var dsos = _rng.Next(_trlCalculation.SignificantDayRange.Lo, _trlCalculation.SignificantDayRange.Hi);
                var reportType = 1;
                var symptomatic = (InfectiousPeriodType)_rng.Next(0, 1);
                result[i] = new EksCreateJobInputEntity
                {
                    KeyData = _rng.NextByteArray(UniversalConstants.DailyKeyDataByteCount),
                    RollingPeriod = UniversalConstants.RollingPeriodRange.Hi,
                    RollingStartNumber = rsn,
                    DaysSinceSymptomsOnset = dsos,
                    TransmissionRiskLevel = _trlCalculation.Calculate(dsos),
                    Symptomatic = dsos < 0 ? InfectiousPeriodType.Symptomatic : symptomatic, // If dsos < 0 then always symptomatic. Else add the random value
                    ReportType = reportType
                };
            }

            return result;
        }
    }
}