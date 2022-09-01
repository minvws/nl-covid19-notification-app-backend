// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.Stuffing
{
    public class WriteStuffingToDiagnosisKeys : IWriteStuffingToDiagnosisKeys
    {
        private readonly DiagnosisKeysDbContext _diagnosisKeysDbContext;
        private readonly EksPublishingJobDbContext _eksPublishingDbContext;
        private readonly IDiagnosticKeyProcessor[] _dkProcessors;
        public WriteStuffingToDiagnosisKeys(DiagnosisKeysDbContext diagnosisKeysDbContext, EksPublishingJobDbContext eksPublishingDbContext, IDiagnosticKeyProcessor[] dkProcessors)
        {
            _diagnosisKeysDbContext = diagnosisKeysDbContext ?? throw new ArgumentNullException(nameof(diagnosisKeysDbContext));
            _eksPublishingDbContext = eksPublishingDbContext ?? throw new ArgumentNullException(nameof(eksPublishingDbContext));
            _dkProcessors = dkProcessors ?? throw new ArgumentNullException(nameof(eksPublishingDbContext));
        }

        public void Execute()
        {
            var stuffing = _eksPublishingDbContext.Set<EksCreateJobInputEntity>()
                .Where(x => x.TekId == null && x.Used)
                .OrderBy(x => x.TekId)
                .Select(x =>
                    new DiagnosisKeyEntity
                    {
                        PublishedLocally = true,
                        DailyKey = new DailyKey(x.KeyData, x.RollingStartNumber, x.RollingPeriod),
                        Local = new LocalTekInfo
                        {
                            TransmissionRiskLevel = x.TransmissionRiskLevel,
                            DaysSinceSymptomsOnset = x.DaysSinceSymptomsOnset,
                            ReportType = x.ReportType
                        }
                    })
                .ToArray();

            var items = stuffing
                .Select(x => (DkProcessingItem)new DkProcessingItem
                {
                    DiagnosisKey = x,
                    Metadata = new Dictionary<string, object>()
                }).ToArray();

            items = _dkProcessors.Execute(items);
            var results = items.Select(x => x.DiagnosisKey).ToList(); //Can't get rid of compiler warning.

            if (results.Any())
            {
                _diagnosisKeysDbContext.BulkInsertBinaryCopy(results);
            }
        }
    }
}
