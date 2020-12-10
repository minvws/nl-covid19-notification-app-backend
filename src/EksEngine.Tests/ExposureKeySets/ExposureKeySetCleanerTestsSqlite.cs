using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Tests.ExposureKeySets
{
    [Trait("db", "mem")]
    public class ExposureKeySetCleanerTestsSqlite : ExposureKeySetCleanerTests
    {
        public ExposureKeySetCleanerTestsSqlite() : base(
            new SqliteInMemoryDbProvider<ContentDbContext>()
        )
        { }
    }
}