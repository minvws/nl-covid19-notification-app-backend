// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Tests.Controllers
{
    [Trait("db", "ss")]
    public class WorkflowControllerPostKeysDiagnosticTestsSqlserver : WorkflowControllerPostKeysDiagnosticTests
    {
        private const string Prefix = nameof(WorkflowControllerPostKeysDiagnosticTests) + "_";
        public WorkflowControllerPostKeysDiagnosticTestsSqlserver() : base(
            new SqlServerDbProvider<WorkflowDbContext>(Prefix + "W")
        )
        { }
    }
}
