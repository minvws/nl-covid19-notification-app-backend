using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Tests.ExposureKeySets
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