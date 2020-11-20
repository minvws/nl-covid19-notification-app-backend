// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.IksOutbound.Publishing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Interop.TestData
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
                ReportType = Eu.Interop.EfgsReportType.ConfirmedTest,
                Origin = "DE",
                DaysSinceSymtpomsOnset = 1,
                Value = new DailyKey
                {
                    RollingStartNumber = BaseTime.Date.ToRollingStartNumber(),
                    RollingPeriod = UniversalConstants.RollingPeriodMax,
                    KeyData = new byte[UniversalConstants.DailyKeyDataLength]
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