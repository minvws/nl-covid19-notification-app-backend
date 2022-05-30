// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;

namespace DbProvision
{
    public class ContentPublisher
    {
        private readonly ILogger _logger;
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly ContentValidator _validator;
        private readonly ContentDbContext _contentDbContext;

        private readonly ContentInsertDbCommand _insertDbCommand;

        public ContentPublisher(ILogger<ContentPublisher> logger, IUtcDateTimeProvider dateTimeProvider, ContentValidator validator, ContentDbContext contentDbContext, ContentInsertDbCommand insertDbCommand)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _contentDbContext = contentDbContext ?? throw new ArgumentNullException(nameof(contentDbContext));
            _insertDbCommand = insertDbCommand ?? throw new ArgumentNullException(nameof(insertDbCommand));
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
                Json = File.ReadAllText(args[1])
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
                return ContentTypes.RiskCalculationParametersV3;
            }

            if (arg.Equals("-b", StringComparison.InvariantCultureIgnoreCase))
            {
                return ContentTypes.ResourceBundleV3;
            }

            throw new InvalidOperationException("Cannot parse Content Type.");
        }
    }
}
