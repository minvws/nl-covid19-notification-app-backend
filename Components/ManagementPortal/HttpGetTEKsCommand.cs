// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.GeneratedGaenFormat;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ManagementPortal
{
    public class HttpGetTEKsCommand
    {

        private readonly WorkflowDbContext _WorkflowDbContext;
        
        public HttpGetTEKsCommand(WorkflowDbContext workflowDbContext)
        {
            _WorkflowDbContext = workflowDbContext;
        }

        public async Task<List<TemporaryExposureKeyEntity>> Execute()
        {
            // TODO: add pagination
            var teks = _WorkflowDbContext.TemporaryExposureKeys.Include(x => x.Owner).ToList();
            return teks;

        } 
        
    }
}