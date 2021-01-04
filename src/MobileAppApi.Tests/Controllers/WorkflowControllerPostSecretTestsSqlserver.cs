using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Tests.Controllers
{
    [Trait("db", "ss")]
    public class WorkflowControllerPostSecretTestsSqlserver : WorkflowControllerPostSecretTests
    {
        private const string Prefix = nameof(WorkflowControllerPostSecretTests) + "_";
        public WorkflowControllerPostSecretTestsSqlserver() : base(
            new SqlServerDbProvider<WorkflowDbContext>(Prefix + "W")
        )
        { }
    }
}