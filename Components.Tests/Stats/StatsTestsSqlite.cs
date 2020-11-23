using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Stats
{
    [Trait("db", "mem")]
    public class StatsTestsSqlite : StatsTests
    {
        public StatsTestsSqlite() : base(
            new SqliteInMemoryDbProvider<WorkflowDbContext>(),
            new SqliteInMemoryDbProvider<StatsDbContext>()
        )
        { }
    }
}