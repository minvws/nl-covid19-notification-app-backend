using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Content
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