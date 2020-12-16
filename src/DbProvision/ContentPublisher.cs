using System;
using System.IO;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;

namespace DbProvision
{
    public class ContentPublisher
    {
        private readonly PublishContentLoggingExtensions _Logger;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly ContentValidator _Validator;
        private readonly ContentDbContext _ContentDbContext;

        private readonly ContentInsertDbCommand _InsertDbCommand;
        private readonly Func<ContentInsertDbCommand> _InsertDbCommandV3;

        public ContentPublisher(PublishContentLoggingExtensions logger, IUtcDateTimeProvider dateTimeProvider, ContentValidator validator, ContentDbContext contentDbContext, ContentInsertDbCommand insertDbCommand, Func<ContentInsertDbCommand> insertDbCommandV3)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _Validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _ContentDbContext = contentDbContext ?? throw new ArgumentNullException(nameof(contentDbContext));
            _InsertDbCommand = insertDbCommand ?? throw new ArgumentNullException(nameof(insertDbCommand));
            _InsertDbCommandV3 = insertDbCommandV3 ?? throw new ArgumentNullException(nameof(insertDbCommandV3));
        }

        public async Task ExecuteAsync(string[] args)
        {
            if (args.Length < 2)
            {
                throw new ArgumentException("Not enough args.");
            }

            var contentArgs = new ContentArgs
            {
                Release = _DateTimeProvider.Snapshot,
                ContentType = ParseContentType(args[0]),
                Json = File.ReadAllText(args[1])
            };

            if (!_Validator.IsValid(contentArgs))
                throw new InvalidOperationException("Content not valid.");

            _Logger.WriteStartWriting(contentArgs.ContentType);

            if (contentArgs.ContentType == ContentTypes.ResourceBundleV3) //needs direct signing with ev-cert
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

            if (arg.Equals("-b", StringComparison.InvariantCultureIgnoreCase))
                return ContentTypes.ResourceBundle;

            if (arg.Equals("-b2", StringComparison.InvariantCultureIgnoreCase))
                return ContentTypes.ResourceBundleV3;

            throw new InvalidOperationException("Cannot parse Content Type.");
        }
    }
}
