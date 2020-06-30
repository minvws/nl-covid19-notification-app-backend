// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using EFCore.BulkExtensions;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

//namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine
//{
//    /// <summary>
//    /// Add database IO to the job
//    /// </summary>
//    public sealed class ExposureKeySetBatchJob : IDisposable
//    {
//        private bool _Disposed;
//        public string JobName { get; }
//        private readonly List<TeksInputEntity> _Used;
//        private readonly List<TemporaryExposureKeyArgs> _KeyBatch = new List<TemporaryExposureKeyArgs>();
//        private readonly IExposureKeySetBatchJobConfig _JobConfig;
//        private readonly IGaenContentConfig _GaenContentConfig;
//        private readonly ITekSource _TekSource;
//        private readonly ExposureKeySetsBatchJobDbContext _JobDbProvider;
//        private readonly IExposureKeySetWriter _Writer;
//        private readonly IExposureKeySetBuilder _SetBuilder;
//        private int _Counter;
//        private readonly DateTime _Start;

//        /// <summary>
//        /// Prod
//        /// </summary>
//        public ExposureKeySetBatchJob(ITekSource tekSource, IDbContextOptionsBuilder jobDbOptionsBuilder, IUtcDateTimeProvider dateTimeProvider,
//            IExposureKeySetWriter eksWriter, IGaenContentConfig gaenContentConfig, /*IJsonExposureKeySetFormatter jsonSetFormatter,*/ IExposureKeySetBuilder setBuilder, IExposureKeySetBatchJobConfig jobConfig)
//        {
//            //_JsonSetFormatter = jsonSetFormatter;
//            _SetBuilder = setBuilder;
//            _JobConfig = jobConfig;
//            _Writer = eksWriter;
//            _GaenContentConfig = gaenContentConfig;
//            _TekSource = tekSource;

//            _Used = new List<TeksInputEntity>(_JobConfig.InputListCapacity);
//            _Start = dateTimeProvider.Now();
//            JobName = $"ExposureKeySetsJob_{_Start:u}".Replace(" ", "_").Replace(":", "_");

//             _JobDbProvider = new ExposureKeySetsBatchJobDbContext(jobDbOptionsBuilder.AddDatabaseName(JobName).Build());
//        }

//        public async Task Execute()
//        {
//            if (_Disposed)
//                throw new ObjectDisposedException(JobName);

//            await CopyInputData();
//            await BuildBatches();
//            await CommitResults();
//        }

//        private async Task BuildBatches()
//        {
//            foreach (var i in _JobDbProvider.Set<TeksInputEntity>().ToArray()) //TODO page or otherwise close the data reader before writing in Build
//            {
//                // TODO use IJsonSerializer
//                var keys = JsonConvert.DeserializeObject<TemporaryExposureKeyContent[]>(i.Content);
                    
//                if (_KeyBatch.Count + keys.Length > _GaenContentConfig.ExposureKeySetCapacity)
//                    await Build();

//                _KeyBatch.AddRange(keys.Select(Map));
//                _Used.Add(i);
//            }

//            if (_KeyBatch.Count > 0)
//                await Build();
//        }

//        private static TemporaryExposureKeyArgs Map(TemporaryExposureKeyContent c)
//            => new TemporaryExposureKeyArgs 
//            { 
//                RollingPeriod = c.RollingPeriod,
//                TransmissionRiskLevel = c.Risk,
//                KeyData = Convert.FromBase64String(c.KeyData),
//                RollingStartNumber = c.RollingStart
//            };

//        private async Task CopyInputData()
//        {
//            var authorisedWorkflows 
//            = _TekSource.Read()
//            .Select(x => new TeksInputEntity
//            {
//                Id = x.Id,
//                Region = x.Region,
//                Content = x.Content,
//                Workflow = x.Workflow
//            })
//            .ToArray();

//            await _JobDbProvider.Database.EnsureCreatedAsync();

//            await using (_JobDbProvider.BeginTransaction())
//            {
//                await _JobDbProvider.BulkInsertAsync(authorisedWorkflows);
//                _JobDbProvider.SaveAndCommit();
//            }
//        }

//        private async Task Build()
//        {
//            var args = _KeyBatch.ToArray();
//            var e = new ExposureKeySetEntity
//            {
//                Created = _Start,
//                CreatingJobName = JobName,
//                CreatingJobQualifier = ++_Counter,
//                //DebugContentJson = _JsonSetFormatter.Build(args),
//                Content = await _SetBuilder.BuildAsync(args)
//            };
//            _KeyBatch.Clear();

//            await using (_JobDbProvider.BeginTransaction())
//            {
//                await _JobDbProvider.AddAsync(e);
//                _JobDbProvider.SaveAndCommit();
//            }

//            await WriteUsed();
//        }

//        private async Task WriteUsed()
//        {
//            foreach (var i in _Used)
//            {
//                i.Used = true;
//            }

//            await using (_JobDbProvider.BeginTransaction())
//            {
//                await _JobDbProvider.BulkUpdateAsync(_Used);
//                _JobDbProvider.SaveAndCommit();
//            }

//            _Used.Clear();
//        }

//        private async Task CommitResults()
//        {
//            _Writer.Write(_JobDbProvider.Set<ExposureKeySetEntity>().ToArray());

//            ////Delete Workflows that were included in a EKS.
//            var q = _JobDbProvider.Set<TeksInputEntity>()
//                .Where(x => x.Used)
//                //.Select(x => new {  })
//                .ToArray()
//                .GroupBy(x => x.Workflow)
//                .ToDictionary(x => x.Key, x => x.Select(y => y.Id).ToArray());

//            var kkll = q.TryGetValue(WorkflowId.KeysLast, out var kl) ? kl : new int[0];
//            var kkff = q.TryGetValue(WorkflowId.KeysFirst, out var kf) ? kf : new int[0];

//            _TekSource.Delete(kkff, kkll);

//            if (_JobConfig.DeleteJobDatabase)
//                await _JobDbProvider.Database.EnsureDeletedAsync();
//        }

//        public void Dispose()
//        {
//            if (_Disposed)
//                return;

//            _Disposed = true;
//            //TODO _JobDatabase?.Dispose();
//        }
//    }
//}