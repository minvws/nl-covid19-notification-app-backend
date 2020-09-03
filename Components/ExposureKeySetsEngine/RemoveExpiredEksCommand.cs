using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public class RemoveExpiredEksCommand
    {
        private readonly ContentDbContext _DbContext;
        private readonly IEksConfig _Config;
        private readonly IUtcDateTimeProvider _Dtp;
        private readonly ILogger<RemoveExpiredEksCommand> _Logger;

        public RemoveExpiredEksCommand(ContentDbContext dbContext, IEksConfig config, IUtcDateTimeProvider dtp, ILogger<RemoveExpiredEksCommand> logger)
        {
            _DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _Config = config ?? throw new ArgumentNullException(nameof(config));
            _Dtp = dtp ?? throw new ArgumentNullException(nameof(dtp));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public RemoveExpiredEksCommandResult Execute()
        {
            var result = new RemoveExpiredEksCommandResult();

            _Logger.LogInformation("Begin removing expired EKS.");

            var cutoff = (_Dtp.Snapshot - TimeSpan.FromDays(_Config.LifetimeDays)).Date;

            using (var tx = _DbContext.BeginTransaction())
            {
                result.Found = _DbContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySet);
                _Logger.LogInformation("Current EKS - Count:{found}.", result.Found);

                var zombies = _DbContext.Content
                    .Where(x => x.Type == ContentTypes.ExposureKeySet && x.Release < cutoff)
                    .Select(x => new { x.PublishingId, x.Release })
                    .ToList();

                result.Zombies = zombies.Count;

                _Logger.LogInformation("Found expired EKS - Cutoff:{cutoff:yyyy-MM-dd}, Count:{count}", cutoff, result.Zombies);
                foreach (var i in zombies)
                    _Logger.LogInformation("Found expired EKS - PublishingId:{PublishingId} Release:{Release}", i.PublishingId, i.Release);

                if (!_Config.CleanupDeletesData)
                {
                    _Logger.LogInformation("Finished EKS cleanup. In safe mode - no deletions.");
                    result.Remaining = result.Found;
                    return result;
                }

                result.GivenMercy = _DbContext.Database.ExecuteSqlInterpolated($"DELETE FROM [Content] WHERE [Type] = {ContentTypes.ExposureKeySet} AND [Release] < {cutoff}");
                tx.Commit();
                //Implicit tx
                result.Remaining = _DbContext.Content.Count(x => x.Type == ContentTypes.ExposureKeySet);
            }

            _Logger.LogInformation("Removed expired EKS - Count:{count}, Remaining:{remaining}", result.GivenMercy, result.Remaining);

            if (result.Reconciliation != 0)
                _Logger.LogError("Reconciliation failed - Found-GivenMercy-Remaining:{remaining}.", result.Reconciliation);

            _Logger.LogInformation("Finished EKS cleanup.");
            return result;
        }
    }
}