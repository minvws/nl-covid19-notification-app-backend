// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig
{
    public class RiskCalculationConfigInsertDbCommand
    {
        private readonly ExposureContentDbContext _DbContextProvider;
        private readonly IPublishingId _PublishingId;

        public RiskCalculationConfigInsertDbCommand(ExposureContentDbContext contextProvider, IPublishingId publishingId)
        {
            _DbContextProvider = contextProvider;
            _PublishingId = publishingId;
        }

        public async Task Execute(RiskCalculationConfigArgs args)
        {
            var e = args.ToEntity();
            e.PublishingId = _PublishingId.Create(e.Content);
            await _DbContextProvider.AddAsync(e);
        }
    }
}