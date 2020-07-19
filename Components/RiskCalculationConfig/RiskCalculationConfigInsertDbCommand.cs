// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig
{
    public class RiskCalculationConfigInsertDbCommand
    {
        private readonly ContentDbContext _DbContextProvider;
        private readonly IContentEntityFormatter _Formatter;
        private readonly IUtcDateTimeProvider _DateTimeProvider;

        public RiskCalculationConfigInsertDbCommand(ContentDbContext dbContextProvider, IContentEntityFormatter formatter, IUtcDateTimeProvider dateTimeProvider)
        {
            _DbContextProvider = dbContextProvider ?? throw new ArgumentNullException(nameof(dbContextProvider));
            _Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        }

        public async Task Execute(RiskCalculationConfigArgs args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            var e = new RiskCalculationContentEntity
            {
                Created = _DateTimeProvider.Now(),
                Release = args.Release
            };
            await _Formatter.Fill(e, args.ToContent());
            await _DbContextProvider.AddAsync(e);
        }
    }
}