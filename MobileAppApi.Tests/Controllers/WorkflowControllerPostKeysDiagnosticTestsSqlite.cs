using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace MobileAppApi.Tests.Controllers
{
    [Trait("db", "mem")]
    public class WorkflowControllerPostKeysDiagnosticTestsSqlite : WorkflowControllerPostKeysDiagnosticTests
    {
        public WorkflowControllerPostKeysDiagnosticTestsSqlite() : base(
            new SqliteInMemoryDbProvider<WorkflowDbContext>()
        )
        { }
    }
}