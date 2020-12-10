using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Tests
{
    [Trait("db", "mem")]
    public class ReSignerTestSqlite : ReSignerTest
    {
        public ReSignerTestSqlite() : base(
            new SqliteInMemoryDbProvider<ContentDbContext>()
        )
        { }
    }
}