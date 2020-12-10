using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Tests
{
    [Trait("db", "ss")]
    public class ReSignerTestSqlserver : ReSignerTest
    {
        private const string Prefix = nameof(ReSignerTest) + "_";
        public ReSignerTestSqlserver() : base(
            new SqlServerDbProvider<ContentDbContext>(Prefix+"C")
        )
        { }
    }
}