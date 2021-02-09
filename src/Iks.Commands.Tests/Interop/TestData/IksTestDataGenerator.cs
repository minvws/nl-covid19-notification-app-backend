// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Publishing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using EfgsReportType = Iks.Protobuf.EfgsReportType;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Tests.Interop.TestData
{
    public class IksTestDataGenerator
    {
        private readonly IDbProvider<IksInDbContext> _IksInDbContextProvider;

        public DateTime BaseTime { get; set; } = DateTime.UtcNow.Date;
    
        public IksTestDataGenerator(IDbProvider<IksInDbContext> iksInDbContextProvider)
        {
            _IksInDbContextProvider = iksInDbContextProvider ?? throw new ArgumentNullException(nameof(iksInDbContextProvider));
        }

        public int CreateIks(int iksCount)
        {
            //Add an IKS or 2.
            var idk = new InteropKeyFormatterArgs
            {
                TransmissionRiskLevel = 1,
                CountriesOfInterest = new[] { "Outer Mongolia" },
                ReportType = EfgsReportType.ConfirmedTest,
                Origin = "DE",
                DaysSinceSymtpomsOnset = 1,
                Value = new DailyKey
                {
                    RollingStartNumber = BaseTime.Date.ToRollingStartNumber(),
                    RollingPeriod = UniversalConstants.RollingPeriodRange.Hi,
                    KeyData = new byte[UniversalConstants.DailyKeyDataByteCount]
                }
            };

            var input = Enumerable.Range(0, iksCount).Select(_ =>
                new IksInEntity
                {
                    Created = BaseTime,
                    BatchTag = "argle",
                    Content = new IksFormatter().Format(new[] { idk }),
                    //Accepted = 
                }).ToArray();

            var iksInDb = _IksInDbContextProvider.CreateNew();
            iksInDb.Received.AddRange(input);
            iksInDb.SaveChanges();

            return iksCount;
        }
    }
}