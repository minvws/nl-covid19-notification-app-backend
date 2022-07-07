// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Tests.Controllers
{
    [Trait("db", "ss")]
    public class WorkflowControllerPostKeysDiagnosticTestsSqlserver : WorkflowControllerPostKeysDiagnosticTests, IDisposable
    {
        private const string Prefix = nameof(WorkflowControllerPostKeysDiagnosticTests) + "_";
        private static DbConnection connection;

        public WorkflowControllerPostKeysDiagnosticTestsSqlserver() : base(
            new DbContextOptionsBuilder<WorkflowDbContext>().UseNpgsql(CreateSqlDatabase("W")).Options
        )
        { }
        private static DbConnection CreateSqlDatabase(string suffix)
        {
            var csb = new SqlConnectionStringBuilder($"Data Source=.;Initial Catalog={Prefix + suffix};Integrated Security=True")
            {
                MultipleActiveResultSets = true
            };

            connection = new SqlConnection(csb.ConnectionString);
            return connection;
        }

        public void Dispose() => connection.Dispose();
    }
}
