// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands
{
    public class EksJobContentWriter : IEksJobContentWriter
    {
        private readonly Func<ContentDbContext> _ContentDbContext;
        private readonly Func<EksPublishingJobDbContext> _PublishingDbContext;
        private readonly IPublishingIdService _PublishingIdService;
        private readonly EksJobContentWriterLoggingExtensions _Logger;

        public EksJobContentWriter(Func<ContentDbContext> contentDbContext, Func<EksPublishingJobDbContext> publishingDbContext, IPublishingIdService publishingIdService, EksJobContentWriterLoggingExtensions logger)
        {
            _ContentDbContext = contentDbContext ?? throw new ArgumentNullException(nameof(contentDbContext));
            _PublishingDbContext = publishingDbContext ?? throw new ArgumentNullException(nameof(publishingDbContext));
            _PublishingIdService = publishingIdService ?? throw new ArgumentNullException(nameof(publishingIdService));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ExecuteAsync()
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