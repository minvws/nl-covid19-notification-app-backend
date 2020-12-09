using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Workflow
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