using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Tests.Workflow
{
    [Trait("db", "ss")]
    public class WorkflowCleanerTestsSqlserver : WorkflowCleanerTests
    {
        private const string Prefix = nameof(WorkflowCleanerTests) + "_";
        public WorkflowCleanerTestsSqlserver() : base(
            new SqlServerDbProvider<WorkflowDbContext>(Prefix + "W")
        )
        { }
    }
}