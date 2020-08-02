// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps
{
    public class ContentDatabaseCreateCommand
    {
        private readonly ContentDbContext _DbContextProvider;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly ContentValidator _Validator;
        private readonly ContentInsertDbCommand _InsertDbCommand;

        public ContentDatabaseCreateCommand(ContentDbContext dbContextProvider, IUtcDateTimeProvider dateTimeProvider, ContentValidator validator, ContentInsertDbCommand insertDbCommand)
        {
            _DbContextProvider = dbContextProvider ?? throw new ArgumentNullException(nameof(dbContextProvider));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _Validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _InsertDbCommand = insertDbCommand ?? throw new ArgumentNullException(nameof(insertDbCommand));
        }

        public async Task Execute(bool nuke)
        {
            if (nuke)
                await _DbContextProvider.Database.EnsureDeletedAsync();
            await _DbContextProvider.Database.EnsureCreatedAsync();
        }

        public async Task AddExampleContent()
        {
            await using var tx = await _DbContextProvider.Database.BeginTransactionAsync();
            await Write(ContentTypes.RiskCalculationParameters, "RiskCalcDefaults.json");
            await Write(ContentTypes.AppConfig, "AppConfigDefaults.json");
            _DbContextProvider.SaveAndCommit();
        }

        private string ReadFromResource(string resourceName)
        {
            var a = System.Reflection.Assembly.GetExecutingAssembly();
            var manifestResourceStream = a.GetManifestResourceStream($"NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Resources.{resourceName}");

            if (manifestResourceStream == null)
                throw new InvalidOperationException("Resource not found.");

            using var s = new StreamReader(manifestResourceStream);
            return s.ReadToEnd();
        }

        private async Task Write(string type, string resource)
        {
            var rcd = ReadFromResource(resource);
            var args = new ContentArgs
            {
                Release = _DateTimeProvider.Snapshot,
                ContentType = type,
                Json = rcd
            };
            if (!_Validator.IsValid(args)) throw new InvalidOperationException();
            await _InsertDbCommand.Execute(args);
        }
    }
}
