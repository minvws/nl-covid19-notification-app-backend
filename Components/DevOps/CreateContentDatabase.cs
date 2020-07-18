// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AppConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps
{
    public class CreateContentDatabase
    {
        private readonly ExposureContentDbContext _DbContextProvider;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly StandardContentEntityFormatter _Formatter;
        private readonly IJsonSerializer _JsonSerializer;

        public CreateContentDatabase(IConfiguration configuration, IUtcDateTimeProvider dateTimeProvider, IContentSigner signer, IJsonSerializer jsonSerializer)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            _JsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));

            var config = new StandardEfDbConfig(configuration, "Content");
            var builder = new SqlServerDbContextOptionsBuilder(config);
            _DbContextProvider = new ExposureContentDbContext(builder.Build());
            _Formatter = new StandardContentEntityFormatter(new ZippedSignedContentFormatter(signer), new StandardPublishingIdFormatter(), jsonSerializer);
        }

        public CreateContentDatabase(ExposureContentDbContext dbContextProvider, IUtcDateTimeProvider dateTimeProvider, IContentSigner signer, IJsonSerializer jsonSerializer)
        {
            _JsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _DbContextProvider = dbContextProvider ?? throw new ArgumentNullException(nameof(dbContextProvider));
            _Formatter = new StandardContentEntityFormatter(new ZippedSignedContentFormatter(signer), new StandardPublishingIdFormatter(), jsonSerializer);
        }


        public async Task Execute()
        {
            await _DbContextProvider.Database.EnsureDeletedAsync();
            await _DbContextProvider.Database.EnsureCreatedAsync();
        }

        public async Task DropExampleContent()
        {
            await using var tx = await _DbContextProvider.Database.BeginTransactionAsync();
            foreach (var e in _DbContextProvider.AppConfigContent)
                _DbContextProvider.AppConfigContent.Remove(e);

            foreach (var e in _DbContextProvider.RiskCalculationContent)
                _DbContextProvider.RiskCalculationContent.Remove(e);
            _DbContextProvider.SaveAndCommit();
        }

        public async Task AddExampleContent()
        {
            await using var tx = await _DbContextProvider.Database.BeginTransactionAsync();

            var rcd = ReadFromResource<RiskCalculationConfigArgs>("RiskCalcDefaults.json");
            rcd.Release = _DateTimeProvider.Now();
            await Write(rcd);

            var acd = ReadFromResource<AppConfigArgs>("AppConfigDefaults.json");
            acd.Release = _DateTimeProvider.Now();
            await Write(acd);

            _DbContextProvider.SaveAndCommit();
        }

        private T ReadFromResource<T>(string resourceName)
        {
            var a = System.Reflection.Assembly.GetExecutingAssembly();
            var manifestResourceStream = a.GetManifestResourceStream($"NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps.{resourceName}");
            using var s = new StreamReader(manifestResourceStream);
            var jsonString = s.ReadToEnd();
            var args = _JsonSerializer.Deserialize<T>(jsonString);
            return args;
        }

        private async Task Write(RiskCalculationConfigArgs a4)
        {
            var e4 = new RiskCalculationContentEntity
            {
                Release = a4.Release
            };
            await _Formatter.Fill(e4, a4.ToContent());
            await _DbContextProvider.AddAsync(e4);
        }

        private async Task Write(AppConfigArgs a4)
        {
            var e4 = new AppConfigContentEntity
            {
                Release = a4.Release
            };
            await _Formatter.Fill(e4, a4.ToContent());
            await _DbContextProvider.AddAsync(e4);
        }
    }
}
