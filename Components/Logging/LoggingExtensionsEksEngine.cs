// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.EksEngine
{
	public static class LoggingExtensionsEksEngine
	{
		public static void WriteStart(this ILogger logger, string jobName)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Started - JobName:{JobName}.",
				LoggingDataEksEngine.Name, LoggingDataEksEngine.Start,
				jobName);
		}

		public static void WriteFinished(this ILogger logger, string jobName)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] {JobName} complete.",
				LoggingDataEksEngine.Name, LoggingDataEksEngine.Finished, jobName);
		}

		public static void WriteNoElevatedPrivs(this ILogger logger, string jobName)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogWarning("[{name}/{id}] JobName} started WITHOUT elevated privileges - errors may occur when signing content.",
				LoggingDataEksEngine.Name, LoggingDataEksEngine.NoElevatedPrivs,
				jobName);
		}

		public static void WriteReconciliationMatchUsable(this ILogger logger, int reconciliationCount)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Reconciliation - Teks in EKSs matches usable input and stuffing - Delta:{ReconcileOutputCount}.",
				LoggingDataEksEngine.Name, LoggingDataEksEngine.ReconciliationTeksMatchInputAndStuffing, 
				reconciliationCount);
		}

		public static void WriteReconciliationMatchCount(this ILogger logger, int reconciliationCount)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Reconciliation - Teks in EKSs matches output count - Delta:{ReconcileEksSumCount}.",
				LoggingDataEksEngine.Name, LoggingDataEksEngine.ReconcilliationTeksMatchOutputCount,
				reconciliationCount);
		}

		public static void WriteCleartables(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogDebug("[{name}/{id}] Clear job tables.",
				LoggingDataEksEngine.Name, LoggingDataEksEngine.ClearJobTables);
		}

		public static void WriteNoStuffingNoTeks(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] No stuffing required - No publishable TEKs.",
				LoggingDataEksEngine.Name, LoggingDataEksEngine.NoStuffingNoTeks);
		}

		public static void WriteNoStuffingMinimumOk(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] No stuffing required - Minimum TEK count OK.",
				LoggingDataEksEngine.Name, LoggingDataEksEngine.NoStuffingMinimumOk);
		}

		public static void WriteStuffingRequired(this ILogger logger, int length)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Stuffing required - Count:{Count}.",
				LoggingDataEksEngine.Name, LoggingDataEksEngine.StuffingRequired,
				length);
		}

		public static void WriteStuffingAdded(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Stuffing added.",
				LoggingDataEksEngine.Name, LoggingDataEksEngine.StuffingAdded);
		}

		public static void WriteBuildEkses(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogDebug("[{name}/{id}] Build EKSs.",
				LoggingDataEksEngine.Name, LoggingDataEksEngine.BuildEkses);
		}

		public static void WriteReadTeks(this ILogger logger, int pageLength)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogDebug("[{name}/{id}] Read TEKs - Count:{Count}.",
				LoggingDataEksEngine.Name, LoggingDataEksEngine.ReadTeks,
				pageLength);
		}

		public static void WritePageFillsToCapacity(this ILogger logger, int capacity)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogDebug("[{name}/{id}] This page fills the EKS to capacity - Capacity:{Capacity}.",
				LoggingDataEksEngine.Name, LoggingDataEksEngine.PageFillsToCapacity,
				capacity);
		}

		public static void WriteRemainingTeks(this ILogger logger, int count)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogDebug("[{name}/{id}] Write remaining TEKs - Count:{Count}.",
				LoggingDataEksEngine.Name, LoggingDataEksEngine.WriteRemainingTeks,
				count);
		}

		public static void WriteAddTeksToOutput(this ILogger logger, int count, int total)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogDebug("[{name}/{id}] dd TEKs to output - Count:{Count}, Total:{OutputCount}.",
				LoggingDataEksEngine.Name, LoggingDataEksEngine.AddTeksToOutput,
				count, total);
		}

		public static void WriteBuildEntry(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogDebug("[{name}/{id}] Build EKS.",
				LoggingDataEksEngine.Name, LoggingDataEksEngine.BuildEntry);
		}

		public static void WriteWritingCurrentEks(this ILogger logger, int qualifier)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Write EKS - Id:{CreatingJobQualifier}.",
				LoggingDataEksEngine.Name, LoggingDataEksEngine.WritingCurrentEks,
				qualifier);
		}

		public static void WriteMarkTekAsUsed(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Mark TEKs as used.",
				LoggingDataEksEngine.Name, LoggingDataEksEngine.MarkTekAsUsed);
		}

		public static void WriteStartReadPage(this ILogger logger, int skipcount, int takecount)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogDebug("[{name}/{id}] Read page - Skip {Skip}, Take {Take}.",
				LoggingDataEksEngine.Name, LoggingDataEksEngine.StartReadPage,
				skipcount, takecount);
		}

		public static void WriteFinishedReadPage(this ILogger logger, int length)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogDebug("[{name}/{id}] Read page - Count:{Count}.",
				LoggingDataEksEngine.Name, LoggingDataEksEngine.FinishReadPage,
				length);
		}

		public static void WriteCommitPublish(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Commit results - publish EKSs.",
				LoggingDataEksEngine.Name, LoggingDataEksEngine.CommitPublish);
		}

		public static void WriteCommitMarkTeks(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Commit results - Mark TEKs as Published.",
				LoggingDataEksEngine.Name, LoggingDataEksEngine.CommitMarkTeks);
		}

		public static void WriteTotalMarked(this ILogger logger, int totalMarked)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Marked as published - Total:{TotalMarked}.",
				LoggingDataEksEngine.Name, LoggingDataEksEngine.TotalMarked,
				totalMarked);
		}

	}
}
