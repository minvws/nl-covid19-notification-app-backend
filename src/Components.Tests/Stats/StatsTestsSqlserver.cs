using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Stats
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