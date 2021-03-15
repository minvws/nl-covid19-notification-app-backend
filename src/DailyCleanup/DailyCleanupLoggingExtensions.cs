// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup
{
    public class DailyCleanupLoggingExtensions
    {
        private const string Name = "DailyCleanup";
        private const int Base = LoggingCodex.DailyCleanup;

        private const int Start = Base;
        private const int Finished = Base + 99;

        private const int EksEngineStarting = Base + 1;
        private const int ManifestEngineStarting = Base + 2;
        private const int DailyStatsCalcStarting = Base + 3;
        private const int ManiFestCleanupStarting = Base + 4;
        private const int EksCleanupStarting = Base + 5;
        private const int WorkflowCleanupStarting = Base + 6;
        private const int ResignerStarting = Base + 7;
        private const int EksV2CleanupStarting = Base + 8;
        private const int ManifestV2CleanupStarting = Base + 9;
        private const int ExpiredIksInCleanupStarting = Base + 10;
        private const int ExpiredIksOutCleanupStarting = Base + 11;
        private const int ManifestV3CleanupStarting = Base + 12;
        private const int ManifestV4CleanupStarting = Base + 13;

        private readonly ILogger _Logger;

        public DailyCleanupLoggingExtensions(ILogger<DailyCleanupLoggingExtensions> logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WriteStart()
        {
            _Logger.LogInformation("[{name}/{id}] Daily cleanup - Starting.",
                Name, Start);
        }

        public void WriteFinished()
        {
            _Logger.LogInformation("[{name}/{id}] Daily cleanup - Finished.",
                Name, Finished);
        }

        public void WriteEksEngineStarting()
        {
            _Logger.LogInformation("[{name}/{id}] Daily cleanup - EKS engine run starting.",
                Name, EksEngineStarting);
        }

        public void WriteManifestEngineStarting()
        {
            _Logger.LogInformation("[{name}/{id}] Daily cleanup - Manifest engine run starting.",
                Name, ManifestEngineStarting);
        }

        public void WriteDailyStatsCalcStarting()
        {
            _Logger.LogInformation("[{name}/{id}] Daily cleanup - Calculating daily stats starting.",
                Name, DailyStatsCalcStarting);
        }

        public void WriteManiFestCleanupStarting()
        {
            _Logger.LogInformation("[{name}/{id}] Daily cleanup - Cleanup Manifests run starting.",
                Name, ManiFestCleanupStarting);
        }

        public void WriteEksCleanupStarting()
        {
            _Logger.LogInformation("[{name}/{id}] Daily cleanup - Cleanup EKS run starting.",
                Name, EksCleanupStarting);
        }

        public void WriteWorkflowCleanupStarting()
        {
            _Logger.LogInformation("[{name}/{id}] Daily cleanup - Cleanup Workflows run starting.",
                Name, WorkflowCleanupStarting);
        }

        public void WriteResignerStarting()
        {
            _Logger.LogInformation("[{name}/{id}] Daily cleanup - Resigning existing v1 content.",
                Name, ResignerStarting);
        }

        public void WriteEksV2CleanupStarting()
        {
            _Logger.LogInformation("[{name}/{id}] Daily cleanup - Cleanup EKSv2 run starting.",
                Name, EksV2CleanupStarting);
        }

        public void WriteManifestV2CleanupStarting()
        {
            _Logger.LogInformation("[{name}/{id}] Daily cleanup - Cleanup ManifestV2 run starting.",
                Name, ManifestV2CleanupStarting);
        }

        public void WriteManifestV3CleanupStarting()
        {
            _Logger.LogInformation("[{name}/{id}] Daily cleanup - Cleanup ManifestV3 run starting.",
                Name, ManifestV3CleanupStarting);
        }

        public void WriteManifestV4CleanupStarting()
        {
            _Logger.LogInformation("[{name}/{id}] Daily cleanup - Cleanup ManifestV4 run starting.",
                Name, ManifestV4CleanupStarting);
        }

        public void WriteSnaphotWorkflowTeksToDks()
        {
            _Logger.LogInformation("[{name}/{id}] Daily cleanup - Snapshot Workflow TEKs to DKs.",
                Name, ManifestV2CleanupStarting);
        }

        public void WriteExpiredIksInCleanupStarting()
        {
            _Logger.LogInformation("[{name}/{id}] Daily cleanup - Cleanup Expired IksIn run starting.",
                Name, ExpiredIksInCleanupStarting);
        }

        public void WriteExpiredIksOutCleanupStarting()
        {
            _Logger.LogInformation("[{name}/{id}] Daily cleanup - Cleanup Expired IksOut run starting.",
                Name, ExpiredIksOutCleanupStarting);
        }
    }
}