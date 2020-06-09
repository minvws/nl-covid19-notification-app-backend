// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;
using System.Security.Cryptography.X509Certificates;
using EFCore.BulkExtensions;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.EscrowTeks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine
{
    public class DbTekSource : ITekSource
    {
        private readonly IDbContextProvider<WorkflowDbContext> _DbContextProvider;

        public DbTekSource(IDbContextProvider<WorkflowDbContext> dbContextProvider)
        {
            _DbContextProvider = dbContextProvider;
        }

        public SourceItem[] Read()
        {
            using (_DbContextProvider.BeginTransaction())
            {
                var kf = _DbContextProvider.Current.Set<KeysFirstTeksWorkflowEntity>()
                    .Where(x => x.Authorised)
                    .Select(y => new SourceItem {Id = y.Id, Content = y.TekContent, Region = y.Region, Workflow = WorkflowId.KeysFirst});

                var kl = _DbContextProvider.Current.Set<KeysLastTeksWorkflowEntity>()
                    .Where(x => x.State == KeysLastWorkflowState.Authorised)
                    .Select(y => new SourceItem {Id = y.Id, Content = y.TekContent, Region = y.Region, Workflow = WorkflowId.KeysLast});

                return kf.Concat(kl)
                    .ToArray();
            }
        }


        /// <summary>
        /// Called directly with no external access to the data provider = do the tx here
        /// </summary>
        public void Delete(int[] kf, int[] kl)
        {
            using (_DbContextProvider.BeginTransaction())
            {
                var die1 = _DbContextProvider.Current.Set<KeysLastTeksWorkflowEntity>()
                    .Where(x => kl.Contains(x.Id)).ToList();

                _DbContextProvider.Current.BulkDeleteAsync(die1);

                _DbContextProvider.Current.SaveChanges();

                var die2 = _DbContextProvider.Current.Set<KeysFirstTeksWorkflowEntity>()
                    .Where(x => kf.Contains(x.Id)).ToList();

                _DbContextProvider.Current.BulkDeleteAsync(die2);

                _DbContextProvider.SaveAndCommit();
            }
        }
    }
}