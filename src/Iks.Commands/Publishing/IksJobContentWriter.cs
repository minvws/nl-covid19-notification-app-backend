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
        private readonly Func<IksOutDbContext> _ContentDbContext;
        private readonly Func<IksPublishingJobDbContext> _PublishingDbContext;
        private readonly ILogger<IksJobContentWriter> _Logger;

        public IksJobContentWriter(Func<IksOutDbContext> contentDbContext, Func<IksPublishingJobDbContext> publishingDbContext, ILogger<IksJobContentWriter> logger)
        {
            _ContentDbContext = contentDbContext ?? throw new ArgumentNullException(nameof(contentDbContext));
            _PublishingDbContext = publishingDbContext ?? throw new ArgumentNullException(nameof(publishingDbContext));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task ExecuteAsyc()
        {
            await using var pdbc = _PublishingDbContext();
            await using (pdbc.BeginTransaction()) //Read consistency
            {
                var move = pdbc.Output.Select(
                    x => new IksOutEntity
                    {
                        Created = x.Created,
                        ValidFor = x.Created,
                        Content = x.Content,
                        Qualifier = x.CreatingJobQualifier
                        //TODO batch id? use qualifier
                    }).ToArray();

                await using var cdbc = _ContentDbContext();
                await using (cdbc.BeginTransaction())
                {
                    cdbc.Iks.AddRange(move);
                    cdbc.SaveAndCommit();
                }

                _Logger.LogInformation("Published EKSs - Count:{Count}.", move.Length);
            }
        }
    }
}