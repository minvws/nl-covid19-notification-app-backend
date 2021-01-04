// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace SigTestFileCreator
{
    public class SigTestFileCreatorLoggingExtensions
    {
        private const string Name = "SigTestFileCreator";
        private const int Base = LoggingCodex.SigtestFileCreator;

        private const int Start = Base;
        private const int Finished = Base + 99;

        private const int NoElevatedPrivs = Base + 1;
        private const int BuildingResultFile = Base + 2;
        private const int SavingResultFile = Base + 3;
        
        private readonly ILogger _Logger;

        public SigTestFileCreatorLoggingExtensions(ILogger<SigTestFileCreatorLoggingExtensions> logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WriteStart(DateTime time)
        {
            _Logger.LogDebug("[{name}/{id}] Key presence Test started ({Time}).",
                Name, Start,
                time);
        }

        public void WriteFinished(string outputLocation)
        {
            _Logger.LogDebug("[{name}/{id}] Key presence test complete.\nResults can be found in: {OutputLocation}.",
                Name, Finished,
                outputLocation);
        }

        public void WriteNoElevatedPrivs()
        {
            _Logger.LogDebug("[{name}/{id}] The test was started WITHOUT elevated privileges - errors may occur when signing content.",
                Name, NoElevatedPrivs);
        }

        public void WriteBuildingResultFile()
        {
            _Logger.LogDebug("[{name}/{id}] Building EKS-engine resultfile.",
                Name, BuildingResultFile);
        }

        public void WriteSavingResultfile()
        {
            _Logger.LogDebug("[{name}/{id}] Saving EKS-engine resultfile.",
                Name, SavingResultFile);
        }
    }

}
