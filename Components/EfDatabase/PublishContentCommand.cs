// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.PublishContent;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase
{
    public class PublishContentCommand
    {
        private readonly ContentValidator _Validator;
        private readonly ContentInsertDbCommand _InsertDbCommand;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly ContentDbContext _ContentDbContext;
        private readonly PublishContentLoggingExtensions _Logger;

        public PublishContentCommand(ContentValidator validator, ContentInsertDbCommand insertDbCommand, IUtcDateTimeProvider dateTimeProvider, ContentDbContext contentDbContext, PublishContentLoggingExtensions logger)
        {
            _Validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _InsertDbCommand = insertDbCommand ?? throw new ArgumentNullException(nameof(insertDbCommand));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _ContentDbContext = contentDbContext ?? throw new ArgumentNullException(nameof(contentDbContext));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Execute(string[] args)
        {
            if (args.Length < 2)
                throw new ArgumentException("Not enough args.");

            var contentArgs = new ContentArgs
            {
                Release = _DateTimeProvider.Snapshot,
                ContentType = ParseContentType(args[0]),
                Json = File.ReadAllText(args[1])
            };

            if (!_Validator.IsValid(contentArgs))
                throw new InvalidOperationException("Content not valid.");

            _Logger.WriteStartWriting(contentArgs.ContentType);

            _ContentDbContext.BeginTransaction();
            await _InsertDbCommand.Execute(contentArgs);
            _ContentDbContext.SaveAndCommit();

            _Logger.WriteFinishedWriting(contentArgs.ContentType);
        }

        private string ParseContentType(string arg)
        {
            if (arg.Equals("-a", StringComparison.InvariantCultureIgnoreCase))
                return ContentTypes.AppConfig;

            if (arg.Equals("-r", StringComparison.InvariantCultureIgnoreCase))
                return ContentTypes.RiskCalculationParameters;

            if (arg.Equals("-b", StringComparison.InvariantCultureIgnoreCase))
                return ContentTypes.ResourceBundle;

            if (arg.Equals("-b3", StringComparison.InvariantCultureIgnoreCase))
                return ContentTypes.ResourceBundleV3;

            throw new InvalidOperationException("Cannot parse Content Type.");
        }
    }
}
