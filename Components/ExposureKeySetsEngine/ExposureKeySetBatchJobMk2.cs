// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine
{
    /// <summary>
    /// Add database IO to the job
    /// </summary>
    public sealed class ExposureKeySetBatchJobMk2 : IDisposable
    {
        private bool _Disposed;
        public string JobName { get; }

        //private readonly IExposureKeySetBatchJobConfig _JobConfig;
        private readonly IGaenContentConfig _GaenContentConfig;

        //private readonly IExposureKeySetWriter _Writer;
        private readonly IExposureKeySetBuilder _SetBuilder;
        private readonly DateTime _Start;

        private int _Counter;
        private readonly List<EksCreateJobInputEntity> _Used;
        private readonly List<TemporaryExposureKeyArgs> _KeyBatch = new List<TemporaryExposureKeyArgs>();

        private readonly WorkflowDbContext _WorkflowDbContext;
        private readonly ExposureContentDbContext _ContentDbContext;

        private readonly IPublishingId _PublishingId;
        private readonly ILogger _Logger;

        public ExposureKeySetBatchJobMk2(/*IExposureKeySetBatchJobConfig jobConfig,*/ IGaenContentConfig gaenContentConfig, IExposureKeySetBuilder builder, WorkflowDbContext workflowDbContext, ExposureContentDbContext contentDbContext, IUtcDateTimeProvider dateTimeProvider, IPublishingId publishingId, ILogger<ExposureKeySetBatchJobMk2> logger)
        {
            //_JobConfig = jobConfig;
            _GaenContentConfig = gaenContentConfig ?? throw new ArgumentNullException(nameof(gaenContentConfig));
            _SetBuilder = builder ?? throw new ArgumentNullException(nameof(builder));
            _WorkflowDbContext = workflowDbContext ?? throw new ArgumentNullException(nameof(workflowDbContext));
            _ContentDbContext = contentDbContext ?? throw new ArgumentNullException(nameof(contentDbContext));
            _PublishingId = publishingId;
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _Used = new List<EksCreateJobInputEntity>(_GaenContentConfig.ExposureKeySetCapacity); //
            _Start = dateTimeProvider.Now();
            JobName = $"ExposureKeySetsJob_{_Start:u}".Replace(" ", "_").Replace(":", "_");
        }

        public async Task Execute(bool useAllKeys = false)
        {
            if (_Disposed)
                throw new ObjectDisposedException(JobName);

            _Logger.LogInformation($"{JobName} started - useAllKeys:{useAllKeys}");
            
            _WorkflowDbContext.EnsureNoChangesOrTransaction();
            _ContentDbContext.EnsureNoChangesOrTransaction();

            await CopyInput(useAllKeys);
            await BuildBatches();
            await CommitResults();

            _Logger.LogInformation($"{JobName} complete.");
        }

        private async Task BuildBatches()
        {
            _Logger.LogDebug($"Build batches.");

            const int size = 100;
            var count = 0;
            var keys = GetInputBatch(count, size); //TODO page or otherwise close the data reader before writing in Build

            while (keys.Length > 0)
            {
                if (_KeyBatch.Count + keys.Length > _GaenContentConfig.ExposureKeySetCapacity)
                    await Build();

                _KeyBatch.AddRange(keys.Select(Map));
                _Used.AddRange(keys);

                count += size;
                keys = GetInputBatch(count, size);
            }

            if (_KeyBatch.Count > 0)
                await Build();
        }

        private static TemporaryExposureKeyArgs Map(EksCreateJobInputEntity c)
            => new TemporaryExposureKeyArgs 
            { 
                RollingPeriod = c.RollingPeriod,
                TransmissionRiskLevel = c.TransmissionRiskLevel,
                KeyData = c.KeyData,
                RollingStartNumber = c.RollingStartNumber
            };

        private async Task Build()
        {
            _Logger.LogDebug($"Build EKS.");

            var args = _KeyBatch.ToArray();
            
            var content = await _SetBuilder.BuildAsync(args);
            var e = new EksCreateJobOutputEntity
            {
                Region = DefaultValues.Region,
                Release = _Start,
                CreatingJobName = JobName,
                CreatingJobQualifier = ++_Counter,
                Content = content, 
            };

            _KeyBatch.Clear();

            await WriteOutput(e);
            await WriteUsed(_Used.ToArray()); 
        }

        private async Task WriteOutput(EksCreateJobOutputEntity e)
        {
            _Logger.LogInformation($"Write EKS {e.CreatingJobQualifier}.");

            await using (_ContentDbContext.BeginTransaction())
            {
                await _ContentDbContext.AddAsync(e);
                _ContentDbContext.SaveAndCommit();
            }
        }

        public void Dispose()
        {
            if (_Disposed)
                return;

            _Disposed = true;
            //TODO _JobDatabase?.Dispose();
        }

        public async Task CopyInput(bool useAllKeys = false)
        {
            _Logger.LogDebug($"Copy input TEKs.");

            await using (_WorkflowDbContext.BeginTransaction())
            {
                await _ContentDbContext.EksInput.BatchDeleteAsync(); //TODO truncate instead
                await _ContentDbContext.EksOutput.BatchDeleteAsync();

                var read = _WorkflowDbContext.TemporaryExposureKeys
                    .Where(x => (x.Owner.Authorised || useAllKeys) && x.PublishingState == PublishingState.Unpublished) 
                    .Select(x => new EksCreateJobInputEntity
                    {
                        Id = x.Id,
                        RollingPeriod = x.RollingPeriod,
                        KeyData = x.KeyData,
                        TransmissionRiskLevel = x.TransmissionRiskLevel,
                        RollingStartNumber = x.RollingStartNumber,
                    }).ToList();

                await using (_ContentDbContext.BeginTransaction())
                {
                    await _ContentDbContext.BulkInsertAsync(read.ToList());
                    _ContentDbContext.SaveAndCommit();
                }
            }
        }

        public EksCreateJobInputEntity[] GetInputBatch(int skip, int take)
        {
            _Logger.LogDebug($"Read input batch - skip {skip}, take {take}.");
            return _ContentDbContext.EksInput.OrderBy(x => x.KeyData).Skip(skip).Take(take).ToArray();
        }

        public async Task WriteUsed(EksCreateJobInputEntity[] used)
        {
            _Logger.LogDebug($"Mark used, count {used.Length}.");

            foreach (var i in used)
            {
                i.Used = true;
            }
            await using (_ContentDbContext.BeginTransaction())
            {
                await _ContentDbContext.BulkUpdateAsync(used);
                _ContentDbContext.SaveAndCommit();
            }
        }

        public async Task CommitResults()
        {
            _Logger.LogInformation($"Commit results - publish EKSs.");

            await using (_ContentDbContext.BeginTransaction())
            {
                var move = _ContentDbContext.EksOutput.Select(
                    x => new ExposureKeySetContentEntity
                    {
                        Release = x.Release,
                        Content = null,
                        ContentTypeName = null,
                        SignedContentTypeName = MediaTypeNames.Application.Zip,
                        SignedContent = x.Content,
                        CreatingJobName = x.CreatingJobName,
                        CreatingJobQualifier = x.CreatingJobQualifier,
                        PublishingId = _PublishingId.Create(x.Content)
                    }).ToArray();
                _ContentDbContext.ExposureKeySetContent.AddRange(move);
                _ContentDbContext.SaveAndCommit();
            }

            _Logger.LogInformation($"Commit results - Mark TEKs as Published.");

            await using (_ContentDbContext.BeginTransaction()) //Read-only
            {
                await using (_WorkflowDbContext.BeginTransaction())
                {
                    var count = 0;
                    var used = _ContentDbContext.Set<EksCreateJobInputEntity>()
                        .Where(x => x.Used)
                        .Skip(count)
                        .Select(x => x.Id)
                        .Take(100)
                        .ToArray();

                    while (used.Length > 0)
                    {
                        var zap = _WorkflowDbContext.TemporaryExposureKeys
                            .Where(x => used.Contains(x.Id))
                            .ToList();

                        foreach (var i in zap)
                        {
                            i.PublishingState = PublishingState.Published;
                        }

                        await _WorkflowDbContext.BulkUpdateAsync(zap, x => x.PropertiesToInclude = new List<string>() {nameof(TemporaryExposureKeyEntity.PublishingState)});

                        count += used.Length;

                        used = _ContentDbContext.Set<EksCreateJobInputEntity>()
                            .Where(x => x.Used)
                            .Skip(count)
                            .Select(x => x.Id)
                            .Take(100)
                            .ToArray();
                    }

                    _WorkflowDbContext.SaveAndCommit();
                }

                _Logger.LogInformation($"Cleanup job tables.");
                await _ContentDbContext.EksInput.BatchDeleteAsync();
                await _ContentDbContext.EksOutput.BatchDeleteAsync();
                _ContentDbContext.SaveAndCommit();
            }
        }
    }
}