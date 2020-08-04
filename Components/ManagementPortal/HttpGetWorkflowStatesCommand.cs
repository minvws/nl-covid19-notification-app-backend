// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ManagementPortal
{
    public class HttpGetWorkflowStatesCommand
    {

        private readonly WorkflowDbContext _WorkflowDbContext;
        
        public HttpGetWorkflowStatesCommand(WorkflowDbContext workflowDbContext)
        {
            _WorkflowDbContext = workflowDbContext;
        }

        public async Task<List<TekReleaseWorkflowStateEntity>> Execute()
        {
            // TODO: add pagination
            var wf = _WorkflowDbContext.KeyReleaseWorkflowStates.ToList();
            return wf;

        } 
        
    }
}