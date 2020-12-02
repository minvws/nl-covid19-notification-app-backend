using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Content
{
    [Trait("db", "mem")]
    public class ManifestV3CreationTestSqlite : ManifestV3CreationTest
    {
        public ManifestV3CreationTestSqlite() : base(
            new SqliteInMemoryDbProvider<ContentDbContext>()
        )
        { }
    }
}