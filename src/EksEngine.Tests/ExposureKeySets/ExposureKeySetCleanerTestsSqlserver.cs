using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.ExposureKeySets
{
    [Trait("db", "ss")]
    public class ExposureKeySetCleanerTestsSqlserver : ExposureKeySetCleanerTests
    {
        private const string Prefix = nameof(ExposureKeySetCleanerTests) + "_";
        public ExposureKeySetCleanerTestsSqlserver() : base(
            new SqlServerDbProvider<ContentDbContext>(Prefix + "C")
        )
        { }
    }
}