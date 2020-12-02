using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace MobileAppApi.Tests.Controllers
{
    [Trait("db", "ss")]
    public class WorkflowControllerPostKeysTestsSqlserver : WorkflowControllerPostKeysTests
    {
        private const string Prefix = nameof(WorkflowControllerPostKeysDiagnosticTests) + "_";
        public WorkflowControllerPostKeysTestsSqlserver() : base(
            new SqlServerDbProvider<WorkflowDbContext>(Prefix + "W")
        )
        { }
    }
}