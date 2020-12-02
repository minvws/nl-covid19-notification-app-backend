using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Workflow
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