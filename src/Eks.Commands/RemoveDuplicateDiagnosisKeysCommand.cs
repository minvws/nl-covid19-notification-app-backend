// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.DiagnosisKeys.Commands
{
    public class RemoveDuplicateDiagnosisKeysCommand : BaseCommand
    {
        private readonly DkSourceDbContext _dkSourceDbContext;

        public RemoveDuplicateDiagnosisKeysCommand(DkSourceDbContext dkSourceDbContext)
        {
            _dkSourceDbContext = dkSourceDbContext ?? throw new ArgumentNullException(nameof(dkSourceDbContext));
        }

        public override async Task<ICommandResult> ExecuteAsync()
        {
            var duplicates = (
                    await _dkSourceDbContext.DiagnosisKeys
                        .AsNoTracking()
                        .Where(p => p.ReadyForCleanup != true)
                        .ToListAsync())
                .GroupBy(p => p, new DiagnosisKeysEntityComparer())
                .Where(group => group.Count() > 1)
                .ToDictionary(p => p.Key, y => y.Select(entity => entity));

            var affectedEntities = MarkDuplicatesPublished(duplicates);
            foreach (var diagnosisKeyEntity in affectedEntities)
            {
                // Mark duplicateEntities ReadyForCleanup
                diagnosisKeyEntity.ReadyForCleanup = diagnosisKeyEntity.PublishedLocally && diagnosisKeyEntity.PublishedToEfgs;
            }

            var cleanableEntities = affectedEntities
                .Where(x => x.ReadyForCleanup.HasValue && x.ReadyForCleanup.Value)
                .ToList();

            if (cleanableEntities.Any())
            {
                var idsToUpdate = string.Join(",", cleanableEntities.Select(x => x.Id.ToString()).ToArray());

                await _dkSourceDbContext.BulkUpdateSqlRawAsync(
                    tableName: "DiagnosisKeys",
                    columnName: "ReadyForCleanup",
                    value: "true",
                    ids: idsToUpdate);
            }

            return null;
        }

        private static List<DiagnosisKeyEntity> MarkDuplicatesPublished(Dictionary<DiagnosisKeyEntity, IEnumerable<DiagnosisKeyEntity>> duplicateEntities)
        {
            var affectedEntityList = new List<DiagnosisKeyEntity>();

            foreach (var duplicateList in duplicateEntities)
            {
                // Get all duplicateEntities ordered by TransmissionRiskLevel; Highest first
                var duplicates = duplicateList.Value.OrderByDescending(p => p.Local.TransmissionRiskLevel).ToList();

                MarkEfgsDuplicates(duplicates, affectedEntityList);
                MarkLocalDuplicates(duplicates, affectedEntityList);
            }

            return affectedEntityList;
        }

        private static void MarkEfgsDuplicates(IReadOnlyCollection<DiagnosisKeyEntity> duplicateEntityList, ICollection<DiagnosisKeyEntity> affectedEntityList)
        {
            // Mark ALL as Published if any has been published
            if (duplicateEntityList.Any(p => p.PublishedToEfgs))
            {
                // Iterate through all unmarked as published
                foreach (var diagnosisKeyEntity in duplicateEntityList.Where(diagnosisKeyEntity => !diagnosisKeyEntity.PublishedToEfgs))
                {
                    diagnosisKeyEntity.PublishedToEfgs = true;

                    // Add diagnosisKeyEntity in affectedEntityList if it isn't in there already
                    if (affectedEntityList.All(p => p.Id != diagnosisKeyEntity.Id))
                    {
                        affectedEntityList.Add(diagnosisKeyEntity);
                    }
                }
            }
            // Mark all except the row with the highest TRL if non are published
            else
            {
                foreach (var diagnosisKeyEntity in duplicateEntityList.Skip(1))
                {
                    // If already marked published, do nothing
                    if (diagnosisKeyEntity.PublishedToEfgs)
                    { continue; }

                    diagnosisKeyEntity.PublishedToEfgs = true;

                    // Add diagnosisKeyEntity in affectedEntityList if it isn't in there already
                    if (affectedEntityList.All(p => p.Id != diagnosisKeyEntity.Id))
                    {
                        affectedEntityList.Add(diagnosisKeyEntity);
                    }
                }
            }
        }

        private static void MarkLocalDuplicates(IReadOnlyCollection<DiagnosisKeyEntity> duplicateEntityList, ICollection<DiagnosisKeyEntity> affectedEntityList)
        {
            // Mark ALL as Published if any has been published
            if (duplicateEntityList.Any(p => p.PublishedLocally))
            {
                // Iterate through all unmarked as published
                foreach (var diagnosisKeyEntity in duplicateEntityList.Where(diagnosisKeyEntity => !diagnosisKeyEntity.PublishedLocally))
                {
                    diagnosisKeyEntity.PublishedLocally = true;

                    // Add diagnosisKeyEntity in affectedEntityList if it isn't in there already
                    if (affectedEntityList.All(p => p.Id != diagnosisKeyEntity.Id))
                    {
                        affectedEntityList.Add(diagnosisKeyEntity);
                    }
                }
            }
            // Mark all except the row with the highest TRL if non are published
            else
            {
                foreach (var diagnosisKeyEntity in duplicateEntityList.Skip(1))
                {
                    if (diagnosisKeyEntity.PublishedLocally)
                    { continue; }

                    diagnosisKeyEntity.PublishedLocally = true;

                    // Add diagnosisKeyEntity in affectedEntityList if it isn't in there already
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
                return y != null && x != null &&
                       x.DailyKey.KeyData.SequenceEqual(y.DailyKey.KeyData) &&
                       x.DailyKey.RollingPeriod == y.DailyKey.RollingPeriod &&
                       x.DailyKey.RollingStartNumber == y.DailyKey.RollingStartNumber;
            }
            public int GetHashCode(DiagnosisKeyEntity obj)
            {
                return obj.DailyKey.KeyData.Aggregate(string.Empty, (s, i) => s + i.GetHashCode(), s => s.GetHashCode());
            }
        }
    }
}
