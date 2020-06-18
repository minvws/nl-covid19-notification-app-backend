// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AppConfig
{
    public class AppConfigInsertDbCommand
    {
        private readonly ExposureContentDbContext _DbContext;
        private readonly IContentEntityFormatter _Formatter;

        public AppConfigInsertDbCommand(ExposureContentDbContext context, IContentEntityFormatter formatter)
        {
            _DbContext = context;
            _Formatter = formatter;
        }

        public async Task Execute(AppConfigArgs args)
        {
            var e = new AppConfigContentEntity
            {
                Release = args.Release
            };
            await _Formatter.Fill(e, args.ToContent());
            await _DbContext.AddAsync(e);
        }
    }
}
