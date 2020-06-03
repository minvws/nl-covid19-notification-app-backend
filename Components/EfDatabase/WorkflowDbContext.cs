// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase
{
    public class WorkflowDbContext : DbContext
    {
        public WorkflowDbContext(DbContextOptions options)
            : base(options)
        {
        }

        [Obsolete]
        public DbSet<KeysFirstWorkflowEntity> KeysFirstWorkflows { get; set; }
        public DbSet<TokenFirstWorkflowEntity> TokenFirstWorkflows { get; set; }
    }
}