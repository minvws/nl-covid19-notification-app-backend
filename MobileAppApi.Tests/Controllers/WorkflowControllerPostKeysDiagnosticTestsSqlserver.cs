using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace MobileAppApi.Tests.Controllers
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