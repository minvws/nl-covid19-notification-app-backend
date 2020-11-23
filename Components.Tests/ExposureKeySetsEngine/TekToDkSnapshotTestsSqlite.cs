using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.ExposureKeySetsEngine
{
    [Trait("db", "mem")]
    public class TekToDkSnapshotTestsSqlite : TekToDkSnapshotTests
    {
        public TekToDkSnapshotTestsSqlite() : base(
            new SqliteInMemoryDbProvider<WorkflowDbContext>(),
            new SqliteInMemoryDbProvider<DkSourceDbContext>(),
            new SqliteWrappedEfExtensions()
        )
        { }
    }
}