using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.ExposureKeySetsEngine
{
    public class Sqlite_TekToDkSnapshotTests : TekToDkSnapshotTests
    {
        public Sqlite_TekToDkSnapshotTests() : base(
            new SqliteInMemoryDbProvider<WorkflowDbContext>(),
            new SqliteInMemoryDbProvider<DkSourceDbContext>(),
            new SqliteWrappedEfExtensions()
        )
        { }
    }
}