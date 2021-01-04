using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands.Tests
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