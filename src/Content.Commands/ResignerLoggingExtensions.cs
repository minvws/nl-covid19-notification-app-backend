// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Text;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    public class ResignerLoggingExtensions
    {
        private const string Name = "Resigner";
        private const int Base = LoggingCodex.Resigner;

        private const int CertNotSpecified = Base + 1;
        private const int Report = Base + 2;
        private const int Finished = Base + 99;

        private readonly ILogger _Logger;

        public ResignerLoggingExtensions(ILogger<ResignerLoggingExtensions> logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WriteFinished()
        {
            _Logger.LogInformation("[{name}/{id}] Re-signing complete.",
                Name, Finished);
        }

        public void WriteCertNotSpecified()
        {
            _Logger.LogWarning("[{name}/{id}] Certificate for re-signing not specified in settings. Re-signing will not run.",
                Name, CertNotSpecified);
        }
        
        public void WriteReport(ContentEntity[]? reportContent)
        {
            if (reportContent == null)
            {
                throw new ArgumentNullException(nameof(reportContent));
            }

            var report = new StringBuilder();
            report.AppendLine($"Re-signing {reportContent.Length} items:");

            foreach (var entry in reportContent)
            {
                report.AppendLine($"PK:{entry.Id} PublishingId:{entry.PublishingId} Created:{entry.Created:O} Release:{entry.Release:O}");
            }

            _Logger.LogInformation("[{name}/{id}] {report}.",
                Name, Report,
                report.ToString());
        }
    }
}
