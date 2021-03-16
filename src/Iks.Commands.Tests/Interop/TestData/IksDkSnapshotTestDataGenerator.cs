// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Tests.Interop.TestData
{
    public class IksDkSnapshotTestDataGenerator
    {
        private readonly IDbProvider<IksPublishingJobDbContext> _IksPublishingDbProvider;

        public IksDkSnapshotTestDataGenerator(IDbProvider<IksPublishingJobDbContext> iksPublishingDbProvider)
        {
            _IksPublishingDbProvider = iksPublishingDbProvider ?? throw new ArgumentNullException(nameof(iksPublishingDbProvider));
        }

        public void Write(IksCreateJobInputEntity[] items)
        {
            var db = _IksPublishingDbProvider.CreateNew();
            db.Input.AddRange(items);
            db.SaveChanges();
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