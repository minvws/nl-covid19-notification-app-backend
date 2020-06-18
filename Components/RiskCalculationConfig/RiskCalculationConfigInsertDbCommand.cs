// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig
{
    public class RiskCalculationConfigInsertDbCommand
    {
        private readonly ExposureContentDbContext _DbContextProvider;
        private readonly IContentEntityFormatter _Formatter;

        public RiskCalculationConfigInsertDbCommand(ExposureContentDbContext contextProvider, IContentEntityFormatter formatter)
        {
            _DbContextProvider = contextProvider;
            _Formatter = formatter;
        }

        public async Task Execute(RiskCalculationConfigArgs args)
        {
            var e = new RiskCalculationContentEntity
            {
                Release = args.Release
            };
            await _Formatter.Fill(e, args.ToContent());
            await _DbContextProvider.AddAsync(e);
        }
    }
}