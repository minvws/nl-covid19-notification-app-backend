// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Newtonsoft.Json;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AgProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine
{
    /// <summary>
    /// Add database IO to the job
    /// </summary>
    public sealed class ExposureKeySetBatchJob : IDisposable
    {
        private bool _Disposed;

        public string JobName { get; }

        private readonly List<WorkflowInputEntity> _Used;
        private readonly List<TemporaryExposureKeyArgs> _KeyBatch = new List<TemporaryExposureKeyArgs>();

        private readonly IExposureKeySetBatchJobConfig _JobConfig;
        private readonly IAgConfig _AgConfig;
        private readonly ITekSource _TekSource;
        private readonly IDbContextProvider<ExposureKeySetsBatchJobDbContext> _JobDbProvider;
        private readonly IExposureKeySetWriter _Writer;
        private readonly IExposureKeySetBuilder _AgSetBuilder;

        private int _Counter;
        private readonly DateTime _Start;

        /// <summary>
        /// Prod
        /// </summary>
        public ExposureKeySetBatchJob(ITekSource tekSource, IDbContextProvider<ExposureKeySetsBatchJobDbContext> jobDbOptionsBuilder, IUtcDateTimeProvider dateTimeProvider,
            IExposureKeySetWriter eksWriter, IAgConfig agConfig, IExposureKeySetBuilder agSetBuilder, IExposureKeySetBatchJobConfig jobConfig)
        {
            _JobConfig = jobConfig;
            _Used = new List<WorkflowInputEntity>(_JobConfig.InputListCapacity);
            _Start = dateTimeProvider.Now();
            JobName = $"ExposureKeySetsJob_{_Start:u}".Replace(" ", "_");
            _JobDbProvider = jobDbOptionsBuilder;
            _AgConfig = agConfig;
            _TekSource = tekSource;
            _AgSetBuilder = agSetBuilder;
            
            _Writer = eksWriter;
        }
        public async Task Execute()
        {
            if (_Disposed)
                throw new ObjectDisposedException(JobName);

            await CopyInputData();
            await BuildBatches();
            await CommitResults();
        }

        private async Task BuildBatches()
        {
            foreach (var i in _JobDbProvider.Current.Set<WorkflowInputEntity>())
            {
                var keys = JsonConvert.DeserializeObject<TemporaryExposureKeyContent[]>(i.Content); //TODO may need an envelope to simplify
                    
                if (_KeyBatch.Count + keys.Length > _AgConfig.ExposureKeySetCapacity)
                    await Build();

                _KeyBatch.AddRange(keys.Select(Map));
                _Used.Add(i);
            }

            if (_KeyBatch.Count > 0)
                await Build();
        }

        private TemporaryExposureKeyArgs Map(TemporaryExposureKeyContent c)
            => new TemporaryExposureKeyArgs 
            { 
                RollingPeriod = c.RollingPeriod,
                TransmissionRiskLevel = c.Risk,
                KeyData = Convert.FromBase64String(c.DailyKey),
                RollingStartNumber = c.RollingStart
            };

        private async Task CopyInputData()
        {
            var authorisedWorkflows 
            = _TekSource.Read()
            .Select(x => new WorkflowInputEntity
            {
                Id = x.Id,
                Region = x.Region,
                Content = x.Content
            })
            .ToArray();

            await using var tx = await _JobDbProvider.Current.Database.BeginTransactionAsync();
            await _JobDbProvider.Current.BulkInsertAsync(authorisedWorkflows);
            await _JobDbProvider.Current.SaveChangesAsync();
            await tx.CommitAsync();
        }

        private async Task Build()
        {
            var args = _KeyBatch.ToArray();
            var e = new ExposureKeySetEntity
            {
                Created = _Start,
                CreatingJobName = JobName,
                CreatingJobQualifier = ++_Counter,
                //DebugContentJson = _JsonSetFormatter.Build(args),
                AgContent = await _AgSetBuilder.BuildAsync(args)
            };
            _KeyBatch.Clear();

            await using (var tx = _JobDbProvider.BeginTransaction())
            {
                await _JobDbProvider.Current.AddAsync(e);
                await tx.CommitAsync();
            }

            await WriteUsed();
        }

        private async Task WriteUsed()
        {
            foreach (var i in _Used)
            {
                i.Used = true;
            }

            await using (var tx = _JobDbProvider.BeginTransaction())
            {
                await _JobDbProvider.Current.BulkUpdateAsync(_Used);
                await _JobDbProvider.Current.SaveChangesAsync();
                await tx.CommitAsync();
            }

            _Used.Clear();
        }

        private async Task CommitResults()
        {
            _Writer.Write(_JobDbProvider.Current.Set<ExposureKeySetEntity>().ToArray());

            ////Delete Workflows that were included in a EKS.
            var q = _JobDbProvider.Current.Set<WorkflowInputEntity>()
                .Where(x => x.Used)
                .Select(x => x.Id)
                .ToArray(); //TODO prefer not

            _TekSource.Delete(q);

            if (_JobConfig.DeleteJobDatabase)
                await _JobDbProvider.Current.Database.EnsureDeletedAsync();
        }

        public void Dispose()
        {
            if (_Disposed)
                return;

            _Disposed = true;
            //TODO _JobDatabase?.Dispose();
        }
    }
}