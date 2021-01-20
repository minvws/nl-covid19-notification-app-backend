using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Cleanup
{
    public class RemoveExpiredIksOutCommand
    {
        private readonly Func<IksOutDbContext> _DbContextProvider;
        private readonly RemoveExpiredIksLoggingExtensions _Logger;

        private readonly IUtcDateTimeProvider _UtcDateTimeProvider;
        private RemoveExpiredIksCommandResult _Result;
        private readonly IIksCleaningConfig _IksCleaningConfig;

        public RemoveExpiredIksOutCommand(Func<IksOutDbContext> dbContext, RemoveExpiredIksLoggingExtensions logger,  IUtcDateTimeProvider utcDateTimeProvider, IIksCleaningConfig iksCleaningConfig)
        {
            _DbContextProvider = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _UtcDateTimeProvider = utcDateTimeProvider ?? throw new ArgumentNullException(nameof(utcDateTimeProvider));
            _IksCleaningConfig = iksCleaningConfig;
        }

        /// <summary>
        /// Manifests are updated regularly.
        /// </summary>
        public async Task<RemoveExpiredIksCommandResult> ExecuteAsync()
        {
            if (_Result != null)
            {
                throw new InvalidOperationException("Object already used.");
            }

            _Logger.WriteStart("IksOut");

            _Result = new RemoveExpiredIksCommandResult();
            
            var lifetimeDays = _IksCleaningConfig.LifetimeDays;
            var cutoff = (_UtcDateTimeProvider.Snapshot - TimeSpan.FromDays(lifetimeDays)).Date;

            using (var dbc = _DbContextProvider())
            {
                using (var tx = dbc.BeginTransaction())
                {
                    _Result.Found = dbc.Iks.Count();
                    _Logger.WriteCurrentIksFound(_Result.Found);

                    var zombies = dbc.Iks
                        .Where(x => x.Created < cutoff)
                        .Select(x => new { x.Id, x.Created })
                        .ToList();

                    _Result.Zombies = zombies.Count;

                    _Logger.WriteTotalIksFound(cutoff, _Result.Zombies);

                    // DELETE FROM IksIn.dbo.IksIn WHERE Created < (today - 14-days)
                    _Result.GivenMercy = await dbc.Database.ExecuteSqlRawAsync($"DELETE FROM {TableNames.IksOut} WHERE [Created] < '{cutoff:yyyy-MM-dd HH:mm:ss.fff}';");
                    await tx.CommitAsync();

                    _Result.Remaining = dbc.Iks.Count();
                }
            }

            _Logger.WriteRemovedAmount(_Result.GivenMercy, _Result.Remaining);

            if (_Result.Reconciliation != 0)
            {
                _Logger.WriteReconciliationFailed(_Result.Reconciliation);
            }

            _Logger.WriteFinished();
            return _Result;
        }
    }
}
