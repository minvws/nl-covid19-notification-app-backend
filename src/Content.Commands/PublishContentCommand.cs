// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    public class PublishContentCommand
    {
        private readonly ContentValidator _Validator;
        private readonly ContentInsertDbCommand _InsertDbCommand;
        private readonly Func<ContentInsertDbCommand> _InsertDbCommandV3;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly ContentDbContext _ContentDbContext;
        private readonly PublishContentLoggingExtensions _Logger;

        public PublishContentCommand(
            ContentValidator validator,
            ContentInsertDbCommand insertDbCommand,
            Func<ContentInsertDbCommand> insertDbCommandV3,
            IUtcDateTimeProvider dateTimeProvider,
            ContentDbContext contentDbContext,
            PublishContentLoggingExtensions logger)
        {
            _Validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _InsertDbCommand = insertDbCommand ?? throw new ArgumentNullException(nameof(insertDbCommand));
            _InsertDbCommandV3 = insertDbCommandV3 ?? throw new ArgumentNullException(nameof(insertDbCommandV3));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _ContentDbContext = contentDbContext ?? throw new ArgumentNullException(nameof(contentDbContext));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ExecuteAsync(string[] args)
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

            if (contentArgs.ContentType == ContentTypes.ResourceBundleV3 || contentArgs.ContentType == ContentTypes.RiskCalculationParametersV3) //needs direct signing with ev-cert
            {
                _ContentDbContext.BeginTransaction();
                await _InsertDbCommandV3().ExecuteAsync(contentArgs);
                _ContentDbContext.SaveAndCommit();
            }
            else
            {
                _ContentDbContext.BeginTransaction();
                await _InsertDbCommand.ExecuteAsync(contentArgs);
                _ContentDbContext.SaveAndCommit();

            }

            _Logger.WriteFinishedWriting(contentArgs.ContentType);
        }

        private string ParseContentType(string arg)
        {
            if (arg.Equals("-a", StringComparison.InvariantCultureIgnoreCase))
                return ContentTypes.AppConfig;

            if (arg.Equals("-r", StringComparison.InvariantCultureIgnoreCase))
                return ContentTypes.RiskCalculationParameters;

            if (arg.Equals("-r2", StringComparison.InvariantCultureIgnoreCase))
                return ContentTypes.RiskCalculationParametersV3;

            if (arg.Equals("-b", StringComparison.InvariantCultureIgnoreCase))
                return ContentTypes.ResourceBundle;

            if (arg.Equals("-b2", StringComparison.InvariantCultureIgnoreCase))
                return ContentTypes.ResourceBundleV3;

            throw new InvalidOperationException("Cannot parse Content Type.");
        }
    }
}
