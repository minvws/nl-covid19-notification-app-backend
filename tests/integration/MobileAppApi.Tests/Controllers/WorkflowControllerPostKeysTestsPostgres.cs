// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using Npgsql;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Tests.Controllers
{
    [Trait("db", "postgres")]
    public class WorkflowControllerPostKeysTestsPostgres : WorkflowControllerPostKeysTests, IDisposable
    {
        private const string Prefix = nameof(WorkflowControllerPostKeysTests) + "_";
        private static DbConnection connection;

        public WorkflowControllerPostKeysTestsPostgres() : base(
            new DbContextOptionsBuilder<WorkflowDbContext>()
                .UseNpgsql(CreateSqlDatabase("w"))
                .UseSnakeCaseNamingConvention()
                .Options
        )
        { }

        private static DbConnection CreateSqlDatabase(string suffix)
        {
            var csb = new NpgsqlConnectionStringBuilder($"Host=localhost;Database={Prefix + suffix}");

            connection = new NpgsqlConnection(csb.ConnectionString);
            return connection;
        }

        public void Dispose() => connection.Dispose();
    }
}
