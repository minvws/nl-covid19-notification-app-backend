using System;
using System.IO;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase
{
    public class WriteFromFile
    {
        private readonly ContentValidator _Validator;
        private readonly ContentInsertDbCommand _InsertDbCommand;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly ContentDbContext _ContentDbContext;

        public WriteFromFile(ContentValidator validator, ContentInsertDbCommand insertDbCommand, IUtcDateTimeProvider dateTimeProvider, ContentDbContext contentDbContext)
        {
            _Validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _InsertDbCommand = insertDbCommand ?? throw new ArgumentNullException(nameof(insertDbCommand));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _ContentDbContext = contentDbContext ?? throw new ArgumentNullException(nameof(contentDbContext));
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
            if (!_Validator.IsValid(contentArgs)) throw new InvalidOperationException("Content not valid.");
            _ContentDbContext.BeginTransaction();
            await _InsertDbCommand.Execute(contentArgs);
            _ContentDbContext.SaveAndCommit();
        }

        string ParseContentType(string arg)
        {
            if (arg.Equals("-a", StringComparison.InvariantCultureIgnoreCase))
                return ContentTypes.AppConfig;

            if (arg.Equals("-r", StringComparison.InvariantCultureIgnoreCase))
                return ContentTypes.RiskCalculationParameters;

            throw new InvalidOperationException("Cannot parse Content Type.");
        }
    }
}