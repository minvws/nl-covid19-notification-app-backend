using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Tests.Content
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