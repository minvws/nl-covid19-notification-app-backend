// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Publishing
{
    public class IksJobContentWriter
    {
        private readonly IksOutDbContext _contentDbContext;
        private readonly IksPublishingJobDbContext _publishingDbContext;
        private readonly ILogger<IksJobContentWriter> _logger;

        public  IksJobContentWriter(IksOutDbContext contentDbContext, IksPublishingJobDbContext publishingDbContext, ILogger<IksJobContentWriter> logger)
        {
            _contentDbContext = contentDbContext ?? throw new ArgumentNullException(nameof(contentDbContext));
            _publishingDbContext = publishingDbContext ?? throw new ArgumentNullException(nameof(publishingDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ExecuteAsync()
        {
            await using (_publishingDbContext.BeginTransaction())
            {
                var move = _publishingDbContext.Output.Select(
                    x => new IksOutEntity
                    {
                        Created = x.Created,
                        ValidFor = x.Created,
                        Content = x.Content,
                        Qualifier = x.CreatingJobQualifier
                    }).ToArray();

                await using (_contentDbContext.BeginTransaction())
                {
                    await _contentDbContext.Iks.AddRangeAsync(move);
                    _contentDbContext.SaveAndCommit();
                }

                _logger.LogInformation("Published EKSs - Count:{Count}.", move.Length);
            }
        }
    }
}
