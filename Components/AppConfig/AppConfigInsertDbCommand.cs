// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AppConfig
{
    public class AppConfigInsertDbCommand
    {
        private readonly ContentDbContext _DbContext;
        private readonly IContentEntityFormatter _Formatter;
        private readonly IUtcDateTimeProvider _DateTimeProvider;


        public AppConfigInsertDbCommand(ContentDbContext dbContext, IContentEntityFormatter formatter, IUtcDateTimeProvider dateTimeProvider)
        {
            _DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _Formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        }

        public async Task Execute(AppConfigArgs args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            var e = new AppConfigContentEntity
            {
                Created= _DateTimeProvider.Now(), //TODO audit stamp
                Release = args.Release
            };
            await _Formatter.Fill(e, args.ToContent());
            await _DbContext.AddAsync(e);
        }
    }
}
