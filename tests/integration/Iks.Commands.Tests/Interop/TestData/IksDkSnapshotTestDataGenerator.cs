// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Tests.Interop.TestData
{
    public class IksDkSnapshotTestDataGenerator
    {
        private readonly IksPublishingJobDbContext _iksPublishingDbContext;

        public IksDkSnapshotTestDataGenerator(IksPublishingJobDbContext iksPublishingDbContext)
        {
            _iksPublishingDbContext = iksPublishingDbContext ?? throw new ArgumentNullException(nameof(iksPublishingDbContext));
        }

        public void Write(IksCreateJobInputEntity[] items)
        {
            _iksPublishingDbContext.Input.AddRange(items);
            _iksPublishingDbContext.SaveChanges();
        }

        public IksCreateJobInputEntity[] GenerateInput(long[] dkids)
        {
            return dkids.Select(x =>
                new IksCreateJobInputEntity
                {
                    DkId = x,
                    DaysSinceSymptomsOnset = 1,
                    DailyKey = new DailyKey(new byte[0], 123, 321),
                    TransmissionRiskLevel = TransmissionRiskLevel.High,
                    CountriesOfInterest = "Elsewhere",
                    ReportType = ReportType.ConfirmedTest,
                    Used = true
                }
            ).ToArray();
        }
    }
}
