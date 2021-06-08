// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands
{
    public class EksEngineLoggingExtensions
    {
        private const string Name = "EksEngine";
        private const int Base = LoggingCodex.EksEngine;

        private const int Start = Base;
        private const int Finished = Base + 99;

        private const int NoElevatedPrivs = Base + 1;
        private const int ReconciliationTeksMatchInputAndStuffing = Base + 2;
        private const int ReconcilliationTeksMatchOutputCount = Base + 3;

        //ClearJobTables()
        private const int ClearJobTables = Base + 4;

        //Stuff()
        private const int NoStuffingNoTeks = Base + 5;
        private const int NoStuffingMinimumOk = Base + 6;
        private const int StuffingRequired = Base + 7;
        private const int StuffingAdded = Base + 8;

        //BuildOutput()
        private const int BuildEkses = Base + 9;
        private const int ReadTeks = Base + 10;
        private const int PageFillsToCapacity = Base + 11;
        private const int WriteRemainingTekCount = Base + 12;

        //AddToOutput()
        private const int AddTeksToOutput = Base + 13;

        //WriteNewEksToOutput()
        private const int BuildEntry = Base + 14;
        private const int WritingCurrentEks = Base + 15;
        private const int MarkTekAsUsed = Base + 16;

        //GetInputPage()
        private const int StartReadPage = Base + 17;
        private const int FinishReadPage = Base + 18;

        //CommitResults()
        private const int CommitPublish = Base + 19;
        private const int CommitMarkTeks = Base + 20;
        private const int TotalMarked = Base + 21;

        private readonly ILogger _logger;

        public EksEngineLoggingExtensions(ILogger<EksEngineLoggingExtensions> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WriteStart(string jobName)
        {
            _logger.LogInformation("[{name}/{id}] Started - JobName:{JobName}.",
                Name, Start,
                jobName);
        }

        public void WriteFinished(string jobName)
        {
            _logger.LogInformation("[{name}/{id}] {JobName} complete.",
                Name, Finished,
                jobName);
        }

        public void WriteNoElevatedPrivs(string jobName)
        {
            _logger.LogWarning("[{name}/{id}] {JobName} started WITHOUT elevated privileges - errors may occur when signing content.",
                Name, NoElevatedPrivs,
                jobName);
        }

        public void WriteReconciliationMatchUsable(int reconciliationCount)
        {
            _logger.LogInformation("[{name}/{id}] Reconciliation - Teks in EKSs matches usable input and stuffing - Delta:{ReconcileOutputCount}.",
                Name, ReconciliationTeksMatchInputAndStuffing,
                reconciliationCount);
        }

        public void WriteReconciliationMatchCount(int reconciliationCount)
        {
            _logger.LogInformation("[{name}/{id}] Reconciliation - Teks in EKSs matches output count - Delta:{ReconcileEksSumCount}.",
                Name, ReconcilliationTeksMatchOutputCount,
                reconciliationCount);
        }

        public void WriteCleartables()
        {
            _logger.LogDebug("[{name}/{id}] Clear job tables.",
                Name, ClearJobTables);
        }

        public void WriteNoStuffingNoTeks()
        {
            _logger.LogInformation("[{name}/{id}] No stuffing required - No publishable TEKs.",
                Name, NoStuffingNoTeks);
        }

        public void WriteNoStuffingMinimumOk()
        {
            _logger.LogInformation("[{name}/{id}] No stuffing required - Minimum TEK count OK.",
                Name, NoStuffingMinimumOk);
        }

        public void WriteStuffingRequired(int length)
        {
            _logger.LogInformation("[{name}/{id}] Stuffing required - Count:{Count}.",
                Name, StuffingRequired,
                length);
        }

        public void WriteStuffingAdded()
        {
            _logger.LogInformation("[{name}/{id}] Stuffing added.",
                Name, StuffingAdded);
        }

        public void WriteBuildEkses()
        {
            _logger.LogDebug("[{name}/{id}] Build EKSs.",
                Name, BuildEkses);
        }

        public void WriteReadTeks(int pageLength)
        {
            _logger.LogDebug("[{name}/{id}] Read TEKs - Count:{Count}.",
                Name, ReadTeks,
                pageLength);
        }

        public void WritePageFillsToCapacity(int capacity)
        {
            _logger.LogDebug("[{name}/{id}] This page fills the EKS to capacity - Capacity:{Capacity}.",
                Name, PageFillsToCapacity,
                capacity);
        }

        public void WriteRemainingTeks(int count)
        {
            _logger.LogDebug("[{name}/{id}] Write remaining TEKs - Count:{Count}.",
                Name, WriteRemainingTekCount,
                count);
        }

        public void WriteAddTeksToOutput(int count, int total)
        {
            _logger.LogDebug("[{name}/{id}] dd TEKs to output - Count:{Count}, Total:{OutputCount}.",
                Name, AddTeksToOutput,
                count, total);
        }

        public void WriteBuildEntry()
        {
            _logger.LogDebug("[{name}/{id}] Build EKS.",
                Name, BuildEntry);
        }

        public void WriteWritingCurrentEks(int qualifier)
        {
            _logger.LogInformation("[{name}/{id}] Write EKS - Id:{CreatingJobQualifier}.",
                Name, WritingCurrentEks,
                qualifier);
        }

        public void WriteMarkTekAsUsed()
        {
            _logger.LogInformation("[{name}/{id}] Mark TEKs as used.",
                Name, MarkTekAsUsed);
        }

        public void WriteStartReadPage(int skipcount, int takecount)
        {
            _logger.LogDebug("[{name}/{id}] Read page - Skip {Skip}, Take {Take}.",
                Name, StartReadPage,
                skipcount, takecount);
        }

        public void WriteFinishedReadPage(int length)
        {
            _logger.LogDebug("[{name}/{id}] Read page - Count:{Count}.",
                Name, FinishReadPage,
                length);
        }

        public void WriteCommitPublish()
        {
            _logger.LogInformation("[{name}/{id}] Commit results - publish EKSs.",
                Name, CommitPublish);
        }

        public void WriteCommitMarkTeks()
        {
            _logger.LogInformation("[{name}/{id}] Commit results - Mark TEKs as Published.",
                Name, CommitMarkTeks);
        }

        public void WriteTotalMarked(int totalMarked)
        {
            _logger.LogInformation("[{name}/{id}] Marked as published - Total:{TotalMarked}.",
                Name, TotalMarked,
                totalMarked);
        }

    }
}
