using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public class RemoveExpiredManifestsCommand
    {
        private readonly Func<ContentDbContext> _DbContextProvider;
        private readonly ILogger<RemoveExpiredManifestsCommand> _Logger;
        private readonly IManifestConfig _ManifestConfig;
        private RemoveExpiredManifestsCommandResult _Result;

        public RemoveExpiredManifestsCommand(Func<ContentDbContext> dbContextProvider, ILogger<RemoveExpiredManifestsCommand> logger, IManifestConfig manifestConfig)
        {
            _DbContextProvider = dbContextProvider ?? throw new ArgumentNullException(nameof(dbContextProvider));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ManifestConfig = manifestConfig ?? throw new ArgumentNullException(nameof(manifestConfig));
        }

        /// <summary>
        /// Manifests are updated regularly.
        /// </summary>
        public async Task<RemoveExpiredManifestsCommandResult> Execute()
        {
            if (_Result != null)
                throw new InvalidOperationException("Object already used.");

            _Result = new RemoveExpiredManifestsCommandResult();

            _Logger.LogInformation("Begin removing expired Manifests - Keep Alive Count:{count}", _ManifestConfig.KeepAliveCount);

            await using (var dbContext = _DbContextProvider())
            await using (var tx = dbContext.BeginTransaction())
            {
                _Result.Found = dbContext.Content.Count();

                var walkingDead = dbContext.Content
                    .Where(x => x.Type == ContentTypes.Manifest)
                    .OrderByDescending(x => x.Release)
                    .Skip(_ManifestConfig.KeepAliveCount)
                    .Select(x => new { x.PublishingId, x.Release })
                    .ToList();

                _Result.WalkingDead = walkingDead.Count;
                _Logger.LogInformation("Removing expired Manifests - Count:{count}", walkingDead.Count);
                foreach (var i in walkingDead)
                    _Logger.LogInformation("Removing expired Manifest - PublishingId:{PublishingId} Release:{Release}", i.PublishingId, i.Release);

                if (walkingDead.Count == 0)
                {
                    _Logger.LogInformation("Finished removing expired Manifests - Nothing to remove.");
                    return _Result;
                }

                _Result.Killed = dbContext.Database.ExecuteSqlInterpolated(
                    $"WITH WalkingDead AS (SELECT Id FROM [Content] WHERE [Type] = 'Manifest' ORDER BY [Release] DESC OFFSET {_ManifestConfig.KeepAliveCount} ROWS) DELETE WalkingDead");

                _Result.Remaining = dbContext.Content.Count();

                tx.Commit();
            }

            _Logger.LogInformation("Finished removing expired Manifests - ExpectedCount:{count} ActualCount:{killCount}", _Result.WalkingDead, _Result.Killed);

            if (_Result.Reconciliation != 0)
                _Logger.LogError("Reconciliation failed removing expired Manifests - Found-Killed-Remaining={reconciliation}.", _Result.Reconciliation);

            if (_Result.DeadReconciliation != 0)
                _Logger.LogError("Reconciliation failed removing expired Manifests - WalkingDead-Killed={deadReconciliation}.", _Result.DeadReconciliation);

            return _Result;
        }
    }
}