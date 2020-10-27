using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.EksJobContentWriter;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine
{
    public class EksJobContentWriter 
    {
        private readonly Func<ContentDbContext> _ContentDbContext;
        private readonly Func<PublishingJobDbContext> _PublishingDbContext;
        private readonly IPublishingIdService _PublishingIdService;
        private readonly ILogger<EksJobContentWriter> _Logger;

        public EksJobContentWriter(Func<ContentDbContext> contentDbContext, Func<PublishingJobDbContext> publishingDbContext, IPublishingIdService publishingIdService, ILogger<EksJobContentWriter> logger)
        {
            _ContentDbContext = contentDbContext ?? throw new ArgumentNullException(nameof(contentDbContext));
            _PublishingDbContext = publishingDbContext ?? throw new ArgumentNullException(nameof(publishingDbContext));
            _PublishingIdService = publishingIdService ?? throw new ArgumentNullException(nameof(publishingIdService));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ExecuteAsyc()
        {
            await using var pdbc = _PublishingDbContext();
            await using (pdbc.BeginTransaction()) //Read consistency
            {
                var move = pdbc.EksOutput.Select(
                    x => new ContentEntity
                    {
                        Created = x.Release,
                        Release = x.Release,
                        ContentTypeName = MediaTypeNames.Application.Zip,
                        Content = x.Content,
                        Type = ContentTypes.ExposureKeySet,
                        PublishingId = _PublishingIdService.Create(x.Content)
                    }).ToArray();

                await using var cdbc = _ContentDbContext();
                await using (cdbc.BeginTransaction())
                {
                    cdbc.Content.AddRange(move);
                    cdbc.SaveAndCommit();
                }

                _Logger.WritePublished(move.Length);
            }
        }
    }
}