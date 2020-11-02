// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.EksEngine
{
    public static class LoggingExtensionsEksEngine
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

        public static void WriteStart(this ILogger logger, string jobName)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Started - JobName:{JobName}.",
                Name, Start,
                jobName);
        }

        public static void WriteFinished(this ILogger logger, string jobName)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] {JobName} complete.",
                Name, Finished, jobName);
        }

        public static void WriteNoElevatedPrivs(this ILogger logger, string jobName)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogWarning("[{name}/{id}] JobName} started WITHOUT elevated privileges - errors may occur when signing content.",
                Name, NoElevatedPrivs,
                jobName);
        }

        public static void WriteReconciliationMatchUsable(this ILogger logger, int reconciliationCount)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Reconciliation - Teks in EKSs matches usable input and stuffing - Delta:{ReconcileOutputCount}.",
                Name, ReconciliationTeksMatchInputAndStuffing,
                reconciliationCount);
        }

        public static void WriteReconciliationMatchCount(this ILogger logger, int reconciliationCount)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Reconciliation - Teks in EKSs matches output count - Delta:{ReconcileEksSumCount}.",
                Name, ReconcilliationTeksMatchOutputCount,
                reconciliationCount);
        }

        public static void WriteCleartables(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogDebug("[{name}/{id}] Clear job tables.",
                Name, ClearJobTables);
        }

        public static void WriteNoStuffingNoTeks(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] No stuffing required - No publishable TEKs.",
                Name, NoStuffingNoTeks);
        }

        public static void WriteNoStuffingMinimumOk(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] No stuffing required - Minimum TEK count OK.",
                Name, NoStuffingMinimumOk);
        }

        public static void WriteStuffingRequired(this ILogger logger, int length)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Stuffing required - Count:{Count}.",
                Name, StuffingRequired,
                length);
        }

        public static void WriteStuffingAdded(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Stuffing added.",
                Name, StuffingAdded);
        }

        public static void WriteBuildEkses(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogDebug("[{name}/{id}] Build EKSs.",
                Name, BuildEkses);
        }

        public static void WriteReadTeks(this ILogger logger, int pageLength)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogDebug("[{name}/{id}] Read TEKs - Count:{Count}.",
                Name, ReadTeks,
                pageLength);
        }

        public static void WritePageFillsToCapacity(this ILogger logger, int capacity)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogDebug("[{name}/{id}] This page fills the EKS to capacity - Capacity:{Capacity}.",
                Name, PageFillsToCapacity,
                capacity);
        }

        public static void WriteRemainingTeks(this ILogger logger, int count)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogDebug("[{name}/{id}] Write remaining TEKs - Count:{Count}.", Name, WriteRemainingTekCount, count);
        }

        public static void WriteAddTeksToOutput(this ILogger logger, int count, int total)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogDebug("[{name}/{id}] dd TEKs to output - Count:{Count}, Total:{OutputCount}.",
                Name, AddTeksToOutput,
                count, total);
        }

        public static void WriteBuildEntry(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogDebug("[{name}/{id}] Build EKS.",
                Name, BuildEntry);
        }

        public static void WriteWritingCurrentEks(this ILogger logger, int qualifier)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Write EKS - Id:{CreatingJobQualifier}.",
                Name, WritingCurrentEks,
                qualifier);
        }

        public static void WriteMarkTekAsUsed(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Mark TEKs as used.",
                Name, MarkTekAsUsed);
        }

        public static void WriteStartReadPage(this ILogger logger, int skipcount, int takecount)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogDebug("[{name}/{id}] Read page - Skip {Skip}, Take {Take}.",
                Name, StartReadPage,
                skipcount, takecount);
        }

        public static void WriteFinishedReadPage(this ILogger logger, int length)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogDebug("[{name}/{id}] Read page - Count:{Count}.",
                Name, FinishReadPage,
                length);
        }

        public static void WriteCommitPublish(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Commit results - publish EKSs.",
                Name, CommitPublish);
        }

        public static void WriteCommitMarkTeks(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Commit results - Mark TEKs as Published.",
                Name, CommitMarkTeks);
        }

        public static void WriteTotalMarked(this ILogger logger, int totalMarked)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Marked as published - Total:{TotalMarked}.",
                Name, TotalMarked,
                totalMarked);
        }

    }
}
