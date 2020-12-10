using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.Commands.Tests.Stats
{
    [Trait("db", "ss")]
    public class StatsTestsSqlserver : StatsTests
    {
        private const string Prefix = nameof(StatsTests) + "_";
        public StatsTestsSqlserver() : base(
            new SqlServerDbProvider<WorkflowDbContext>(Prefix + "W"),
            new SqlServerDbProvider<StatsDbContext>(Prefix + "S")
        )
        { }
    }
}