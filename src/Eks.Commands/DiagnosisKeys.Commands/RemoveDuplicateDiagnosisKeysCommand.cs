// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.DiagnosisKeys.Commands
{
    public class RemoveDuplicateDiagnosisKeysCommand
    {
        private readonly DkSourceDbContext _dkSourceDbContext;

        public RemoveDuplicateDiagnosisKeysCommand(DkSourceDbContext dkSourceDbContext)
        {
            _dkSourceDbContext = dkSourceDbContext ?? throw new ArgumentNullException(nameof(dkSourceDbContext));
        }

        public async Task ExecuteAsync()
        {
            var iksDuplicates = (await _dkSourceDbContext.DiagnosisKeys.AsNoTracking().Where(p => !p.ReadyForCleanup.HasValue || !p.ReadyForCleanup.Value).ToListAsync())
                .GroupBy(p => p, new DiagnosisKeysEntityComparer())
                .Where(group => group.Count() > 1)
                .ToDictionary(p => p.Key, y => y.Select(entity => entity));

            var affectedEntities = MarkDuplicatesPublished(iksDuplicates);
            foreach (var diagnosisKeyEntity in affectedEntities)
            {
                // Mark duplicates ReadyForCleanup
                diagnosisKeyEntity.ReadyForCleanup = diagnosisKeyEntity.PublishedLocally && diagnosisKeyEntity.PublishedToEfgs;
            }

            await _dkSourceDbContext.BulkUpdateAsync(affectedEntities);
        }

        private static List<DiagnosisKeyEntity> MarkDuplicatesPublished(Dictionary<DiagnosisKeyEntity, IEnumerable<DiagnosisKeyEntity>> iksDuplicates)
        {
            var affectedEntityList = new List<DiagnosisKeyEntity>();

            foreach (var iksDuplicateList in iksDuplicates)
            {
                // Get all duplicates ordered by TransmissionRiskLevel; Highest first
                var iks = iksDuplicateList.Value.OrderByDescending(p => p.Local.TransmissionRiskLevel).ToList();

                MarkEfgsDuplicates(iks, affectedEntityList);
                MarkLocalDuplicates(iks, affectedEntityList);
            }

            return affectedEntityList;
        }

        private static void MarkEfgsDuplicates(IReadOnlyCollection<DiagnosisKeyEntity> iks, ICollection<DiagnosisKeyEntity> affectedEntityList)
        {
            // Mark ALL as Published if any has been published
            if (iks.Any(p => p.PublishedToEfgs))
            {
                // Iterate through all unmarked as published
                foreach (var diagnosisKeyEntity in iks.Where(diagnosisKeyEntity => !diagnosisKeyEntity.PublishedToEfgs))
                {
                    diagnosisKeyEntity.PublishedToEfgs = true;

                    if (affectedEntityList.All(p => p.Id != diagnosisKeyEntity.Id))
                    {
                        affectedEntityList.Add(diagnosisKeyEntity);
                    }
                }
            }
            // Mark all except the row with the highest TRL if non are published
            else
            {
                foreach (var diagnosisKeyEntity in iks.Skip(1))
                {
                    // If already marked published, do nothing
                    if (diagnosisKeyEntity.PublishedToEfgs)
                    { continue; }

                    diagnosisKeyEntity.PublishedToEfgs = true;

                    if (affectedEntityList.All(p => p.Id != diagnosisKeyEntity.Id))
                    {
                        affectedEntityList.Add(diagnosisKeyEntity);
                    }
                }
            }
        }
        private static void MarkLocalDuplicates(IReadOnlyCollection<DiagnosisKeyEntity> iks, ICollection<DiagnosisKeyEntity> affectedEntityList)
        {
            // Mark ALL as Published if any has been published
            if (iks.Any(p => p.PublishedLocally))
            {
                // Iterate through all unmarked as published
                foreach (var diagnosisKeyEntity in iks.Where(diagnosisKeyEntity => !diagnosisKeyEntity.PublishedLocally))
                {
                    diagnosisKeyEntity.PublishedLocally = true;

                    if (affectedEntityList.All(p => p.Id != diagnosisKeyEntity.Id))
                    {
                        affectedEntityList.Add(diagnosisKeyEntity);
                    }
                }
            }
            // Mark all except the row with the highest TRL if non are published
            else
            {
                foreach (var diagnosisKeyEntity in iks.Skip(1))
                {
                    if (diagnosisKeyEntity.PublishedLocally)
                    { continue; }

                    diagnosisKeyEntity.PublishedLocally = true;

                    if (affectedEntityList.All(p => p.Id != diagnosisKeyEntity.Id))
                    {
                        affectedEntityList.Add(diagnosisKeyEntity);
                    }
                }
            }
        }

        private struct DiagnosisKeysEntityComparer : IEqualityComparer<DiagnosisKeyEntity>
        {
            public bool Equals(DiagnosisKeyEntity x, DiagnosisKeyEntity y)
            {
                return x.DailyKey.KeyData.SequenceEqual(y.DailyKey.KeyData) && x.DailyKey.RollingPeriod == y.DailyKey.RollingPeriod && x.DailyKey.RollingStartNumber == y.DailyKey.RollingStartNumber;
            }
            public int GetHashCode(DiagnosisKeyEntity obj)
            {
                return obj.DailyKey.KeyData.Aggregate(string.Empty, (s, i) => s + i.GetHashCode(), s => s.GetHashCode());
            }
        }
    }
}
