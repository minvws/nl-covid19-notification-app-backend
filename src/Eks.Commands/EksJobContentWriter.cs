// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands
{
    public class EksJobContentWriter : IEksJobContentWriter
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly EksPublishingJobDbContext _eksPublishingJobDbContext;
        private readonly IPublishingIdService _publishingIdService;
        private readonly EksJobContentWriterLoggingExtensions _logger;

        public EksJobContentWriter(ContentDbContext contentDbContext, EksPublishingJobDbContext eksPublishingJobDbContext, IPublishingIdService publishingIdService, EksJobContentWriterLoggingExtensions logger)
        {
            _contentDbContext = contentDbContext ?? throw new ArgumentNullException(nameof(contentDbContext));
            _eksPublishingJobDbContext = eksPublishingJobDbContext ?? throw new ArgumentNullException(nameof(eksPublishingJobDbContext));
            _publishingIdService = publishingIdService ?? throw new ArgumentNullException(nameof(publishingIdService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ExecuteAsync()
        {
            await using (_eksPublishingJobDbContext.BeginTransaction())
            {
                var moveV12 = _eksPublishingJobDbContext.EksOutput.AsNoTracking().Where(x => x.GaenVersion == GaenVersion.v12).Select(
                    x => new ContentEntity
                    {
                        Created = x.Release,
                        Release = x.Release,
                        ContentTypeName = MediaTypeNames.Application.Zip,
                        Content = x.Content,
                        Type = ContentTypes.ExposureKeySetV2,
                        PublishingId = _publishingIdService.Create(x.Content)
                    }).ToArray();

                var moveV15 = _eksPublishingJobDbContext.EksOutput.AsNoTracking().Where(x => x.GaenVersion == GaenVersion.v15).Select(
                    x => new ContentEntity
                    {
                        Created = x.Release,
                        Release = x.Release,
                        ContentTypeName = MediaTypeNames.Application.Zip,
                        Content = x.Content,
                        Type = ContentTypes.ExposureKeySetV3,
                        PublishingId = _publishingIdService.Create(x.Content)
                    }).ToArray();

                await using (_contentDbContext.BeginTransaction())
                {
                    await _contentDbContext.Content.AddRangeAsync(moveV12);
                    await _contentDbContext.Content.AddRangeAsync(moveV15);
                    _contentDbContext.SaveAndCommit();
                }

                _logger.WritePublished(GaenVersion.v12, moveV12.Length);
                _logger.WritePublished(GaenVersion.v15, moveV15.Length);
            }
        }
    }
}
