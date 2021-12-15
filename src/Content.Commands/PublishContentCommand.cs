// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    public class PublishContentCommand
    {
        private readonly ContentValidator _validator;
        private readonly ContentInsertDbCommand _insertDbCommand;
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly ContentDbContext _contentDbContext;
        private readonly ILogger _logger;

        public PublishContentCommand(
            ContentValidator validator,
            ContentInsertDbCommand insertDbCommand,
            Func<ContentInsertDbCommand> insertDbCommandV3,
            IUtcDateTimeProvider dateTimeProvider,
            ContentDbContext contentDbContext,
            ILogger<PublishContentCommand> logger)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _insertDbCommand = insertDbCommand ?? throw new ArgumentNullException(nameof(insertDbCommand));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _contentDbContext = contentDbContext ?? throw new ArgumentNullException(nameof(contentDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ExecuteAsync(string[] args)
        {
            if (args.Length < 2)
            {
                throw new ArgumentException("Not enough args.");
            }

            var contentArgs = new ContentArgs
            {
                Release = _dateTimeProvider.Snapshot,
                ContentType = ParseContentType(args[0]),
                Json = await File.ReadAllTextAsync(args[1])
            };

            if (!_validator.IsValid(contentArgs))
            {
                throw new InvalidOperationException("Content not valid.");
            }

            _logger.LogDebug("Writing {ContentType} to database.", contentArgs.ContentType);

            _contentDbContext.BeginTransaction();
            await _insertDbCommand.ExecuteAsync(contentArgs);
            _contentDbContext.SaveAndCommit();

            _logger.LogDebug("Done writing {ContentType} to database.", contentArgs.ContentType);
        }

        private ContentTypes ParseContentType(string arg)
        {
            if (arg.Equals("-a", StringComparison.InvariantCultureIgnoreCase))
            {
                return ContentTypes.AppConfigV2;
            }

            if (arg.Equals("-r", StringComparison.InvariantCultureIgnoreCase))
            {
                return ContentTypes.RiskCalculationParametersV2;
            }

            if (arg.Equals("-r2", StringComparison.InvariantCultureIgnoreCase))
            {
                return ContentTypes.RiskCalculationParametersV3;
            }

            if (arg.Equals("-b", StringComparison.InvariantCultureIgnoreCase))
            {
                return ContentTypes.ResourceBundleV2;
            }

            if (arg.Equals("-b2", StringComparison.InvariantCultureIgnoreCase))
            {
                return ContentTypes.ResourceBundleV3;
            }

            throw new InvalidOperationException("Cannot parse Content Type.");
        }
    }
}
