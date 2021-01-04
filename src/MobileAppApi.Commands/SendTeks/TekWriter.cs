// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.SendTeks
{
    public class TekWriter : ITekWriter
    {
        private readonly WorkflowDbContext _DbContextProvider;

        public TekWriter(WorkflowDbContext dbContextProvider)
        {
            _DbContextProvider = dbContextProvider ?? throw new ArgumentNullException(nameof(dbContextProvider));
        }

        public async Task ExecuteAsync(TekWriteArgs args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            var entities = args.NewItems.Select(Mapper.MapToEntity).ToArray();
            
            foreach (var e in entities)
                e.Owner = args.WorkflowStateEntityEntity;

            await _DbContextProvider.TemporaryExposureKeys.AddRangeAsync(entities);
        }
    }
}