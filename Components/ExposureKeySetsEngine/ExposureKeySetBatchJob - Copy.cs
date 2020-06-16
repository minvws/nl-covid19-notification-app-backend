// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Newtonsoft.Json;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine
{
    /// <summary>
    /// Add database IO to the job
    /// </summary>
    public sealed class ExposureKeySetBatchJobMk2 : IDisposable
    {
        private bool _Disposed;
        public string JobName { get; }

        private readonly IExposureKeySetBatchJobConfig _JobConfig;
        private readonly IGaenContentConfig _GaenContentConfig;

        private readonly IExposureKeySetWriter _Writer;
        private readonly IExposureKeySetBuilder _SetBuilder;
        private readonly DateTime _Start;

        private int _Counter;
        private readonly List<TeksInputEntity> _Used;
        private readonly List<TemporaryExposureKeyArgs> _KeyBatch = new List<TemporaryExposureKeyArgs>();

        private readonly WorkflowDbContext _WorkflowDbContext;
        private readonly ExposureContentDbContext _ContentDbContext;

        /// <summary>
        /// Prod
        /// </summary>
        public ExposureKeySetBatchJobMk2(ITekSource tekSource, IDbContextOptionsBuilder jobDbOptionsBuilder, IUtcDateTimeProvider dateTimeProvider,
            IExposureKeySetWriter eksWriter, IGaenContentConfig gaenContentConfig, /*IJsonExposureKeySetFormatter jsonSetFormatter,*/ IExposureKeySetBuilder setBuilder, IExposureKeySetBatchJobConfig jobConfig)
        {
            _SetBuilder = setBuilder;
            _JobConfig = jobConfig;
            _Writer = eksWriter;
            _GaenContentConfig = gaenContentConfig;
            //_TekSource = tekSource;
            _Used = new List<TeksInputEntity>(_JobConfig.InputListCapacity);
            _Start = dateTimeProvider.Now();
            JobName = $"ExposureKeySetsJob_{_Start:u}".Replace(" ", "_").Replace(":", "_");
        }

        public async Task Execute()
        {
            if (_Disposed)
                throw new ObjectDisposedException(JobName);

            await CopyInput();
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

        private static TemporaryExposureKeyArgs Map(TeksInputEntity c)
            => new TemporaryExposureKeyArgs 
            { 
                RollingPeriod = c.RollingPeriod,
                TransmissionRiskLevel = c.Risk,
                KeyData = c.KeyData,
                RollingStartNumber = c.RollingStart
            };

        //private async Task CopyInputData()
        //{
        //    var authorisedWorkflows 
        //    = 
        //    .Select(x => new TeksInputEntity
        //    {
        //        Id = x.Id,
        //        Region = x.Region,
        //        Content = x.Content,
        //        Workflow = x.Workflow
        //    })
        //    .ToArray();
        //    await _JobDbProvider.Database.EnsureCreatedAsync();
        //    await using (_JobDbProvider.BeginTransaction())
        //    {
        //        await _JobDbProvider.BulkInsertAsync(authorisedWorkflows);
        //        _JobDbProvider.SaveAndCommit();
        //    }
        //}

        private async Task Build()
        {
            var args = _KeyBatch.ToArray();
            var e = new ExposureKeySetEntity
            {
                Created = _Start,
                CreatingJobName = JobName,
                CreatingJobQualifier = ++_Counter,
                Content = await _SetBuilder.BuildAsync(args)
            };
            _KeyBatch.Clear();

            //TODO wrong type... await WriteEks(e);
            await WriteUsed(_Used.ToArray()); 
        }

        public void Dispose()
        {
            if (_Disposed)
                return;

            _Disposed = true;
            //TODO _JobDatabase?.Dispose();
        }

        public async Task CopyInput()
        {
            await using (_WorkflowDbContext.BeginTransaction())
            {
                //Truncate input and output tables
                var read = _WorkflowDbContext.TemporaryExposureKeys
                    .Where(x => x.Owner.Authorised && x.PublishingState == PublishingState.Unpublished)
                    .Select(x => new TeksInputEntity
                    {
                        Id = x.Id,
                        RollingPeriod = x.RollingPeriod,
                        KeyData = x.KeyData,
                        Risk = x.Risk,
                        RollingStart = x.RollingStart,
                    }).ToList();

                await using (_ContentDbContext.BeginTransaction())
                {
                    await _ContentDbContext.BulkInsertAsync(read.ToList());
                    _WorkflowDbContext.SaveAndCommit();
                }
            }
        }

        public TeksInputEntity[] GetInputBatch(int skip, int take)
        {
            return _ContentDbContext.EksInput.Skip(skip).Take(take).ToArray();
        }

        public async Task WriteEks(ExposureKeySetContentEntity e)
        {
            await using (_ContentDbContext.BeginTransaction())
            {
                await _ContentDbContext.EksOutput.AddAsync(e);
                _ContentDbContext.SaveAndCommit();
            }
        }

        public async Task WriteUsed(TeksInputEntity[] used)
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
                var move = _ContentDbContext.EksOutput.ToArray(); //TODO copy? Cos it's the same DB cos 'policy'
                _ContentDbContext.ExposureKeySetContent.AddRange(move);
                _ContentDbContext.SaveAndCommit();
            }

            await using (_ContentDbContext.BeginTransaction())
            await using (_WorkflowDbContext.BeginTransaction())
            {
                var count = 0;
                var used = _ContentDbContext.Set<TeksInputEntity>()
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

                    await _WorkflowDbContext.BulkUpdateAsync(zap);


                    var q = _ContentDbContext.Set<TeksInputEntity>()
                        .Where(x => x.Used)
                        .Skip(count)
                        .Take(100)
                        .ToArray();

                    count += q.Length;
                }

                _WorkflowDbContext.SaveAndCommit();
            }
        }
    }
}