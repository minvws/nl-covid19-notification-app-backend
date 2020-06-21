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

        public ExposureKeySetBatchJobMk2(/*IExposureKeySetBatchJobConfig jobConfig,*/ IGaenContentConfig gaenContentConfig, IExposureKeySetBuilder builder, WorkflowDbContext workflowDbContext, ExposureContentDbContext contentDbContext, IUtcDateTimeProvider dateTimeProvider, IPublishingId publishingId)
        {
            //_JobConfig = jobConfig;
            _GaenContentConfig = gaenContentConfig;
            _SetBuilder = builder;
            _WorkflowDbContext = workflowDbContext;
            _ContentDbContext = contentDbContext;
            _PublishingId = publishingId;
            _Used = new List<EksCreateJobInputEntity>(_GaenContentConfig.ExposureKeySetCapacity); //
            _Start = dateTimeProvider.Now();
            JobName = $"ExposureKeySetsJob_{_Start:u}".Replace(" ", "_").Replace(":", "_");
        }

        public async Task Execute(bool useAllKeys = false)
        {
            if (_Disposed)
                throw new ObjectDisposedException(JobName);

            _WorkflowDbContext.EnsureNoChangesOrTransaction();
            _ContentDbContext.EnsureNoChangesOrTransaction();

            await CopyInput(useAllKeys);
            await BuildBatches();
            await CommitResults();
        }

        private async Task BuildBatches()
        {
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
            return _ContentDbContext.EksInput.OrderBy(x => x.KeyData).Skip(skip).Take(take).ToArray();
        }

        public async Task WriteUsed(EksCreateJobInputEntity[] used)
        {
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

                await _ContentDbContext.EksInput.BatchDeleteAsync();
                await _ContentDbContext.EksOutput.BatchDeleteAsync();
                _ContentDbContext.SaveAndCommit();
            }
        }
    }
}