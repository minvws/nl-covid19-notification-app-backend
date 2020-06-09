// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;
using EFCore.BulkExtensions;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.EscrowTeks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine
{
    public class DbTekSource : ITekSource
    {
        private readonly WorkflowDbContext _DbContextProvider;

        public DbTekSource(WorkflowDbContext dbContextProvider)
        {
            _DbContextProvider = dbContextProvider;
        }

        public SourceItem[] Read()
        {
            using (_DbContextProvider.BeginTransaction())
            {
                var kf = _DbContextProvider.Set<KeysFirstTeksWorkflowEntity>()
                    .Where(x => x.Authorised)
                    .Select(y => new SourceItem {Id = y.Id, Content = y.TekContent, Region = y.Region, Workflow = WorkflowId.KeysFirst});

                var kl = _DbContextProvider.Set<KeysLastTeksWorkflowEntity>()
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
                var die1 = _DbContextProvider.Set<KeysLastTeksWorkflowEntity>()
                    .Where(x => kl.Contains(x.Id)).ToList();

                _DbContextProvider.BulkDeleteAsync(die1);

                _DbContextProvider.SaveChanges();

                var die2 = _DbContextProvider.Set<KeysFirstTeksWorkflowEntity>()
                    .Where(x => kf.Contains(x.Id)).ToList();

                _DbContextProvider.BulkDeleteAsync(die2);
                _DbContextProvider.SaveAndCommit();
            }
        }
    }
}