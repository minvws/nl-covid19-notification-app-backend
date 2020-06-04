// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;
using EFCore.BulkExtensions;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine
{
    public class DbTekSource : ITekSource
    {
        private readonly DbContextProvider<WorkflowDbContext> _DbContextProvider;

        public DbTekSource(DbContextProvider<WorkflowDbContext> dbContextProvider)
        {
            _DbContextProvider = dbContextProvider;
        }

        public SourceItem[] Read()
            //TODO TX?
            => _DbContextProvider.Current.KeysLastWorkflows
                .Where(x => x.State == TokenFirstWorkflowState.Authorised)
                .Select(x => new SourceItem {Id = x.Id, Content = x.TekContent, Region = x.Region})
                .ToArray();

        public void Delete(int[] things)
        {
            var die = things.Select(x => new KeysLastTekReleaseWorkflowEntity {Id = x}).ToList();
            _DbContextProvider.Current.BulkDeleteAsync(die);
        }
    }
}