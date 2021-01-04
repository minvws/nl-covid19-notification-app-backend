using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Tests.Workflow
{
    [Trait("db", "mem")]
    public class WorkflowCleanerTestsSqlite : WorkflowCleanerTests
    {
        public WorkflowCleanerTestsSqlite() : base(
            new SqliteInMemoryDbProvider<WorkflowDbContext>()
        )
        { }
    }
}