//// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
//// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
//// SPDX-License-Identifier: EUPL-1.2

//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

//namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest
//{
//    public class GetActiveExposureKeySetsListCommand
//    {
//        private readonly ContentDbContext _DbConfig;
//        private readonly IGaenContentConfig _GaenContentConfig;

//        public GetActiveExposureKeySetsListCommand(ContentDbContext dbConfig, IGaenContentConfig gaenContentConfig)
//        {
//            _DbConfig = dbConfig ?? throw new ArgumentNullException(nameof(dbConfig));
//            _GaenContentConfig = gaenContentConfig ?? throw new ArgumentNullException(nameof(gaenContentConfig));
//        }

//        public async Task<string[]> Execute(DateTime now)
//        {
//            var expired = now - TimeSpan.FromDays(_GaenContentConfig.ExposureKeySetLifetimeDays);
//            var result = await _DbConfig.Set<ExposureKeySetContentEntity>()
//                .Where(x => x.Release > expired)
//                .Select(x => x.PublishingId)
//                .ToArrayAsync();

//            return result;
//        }
//    }
//}