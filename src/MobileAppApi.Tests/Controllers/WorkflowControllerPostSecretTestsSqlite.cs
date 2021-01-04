using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Tests.Controllers
{
    [Trait("db", "mem")]
    public class WorkflowControllerPostSecretTestsSqlite : WorkflowControllerPostSecretTests
    {
        public WorkflowControllerPostSecretTestsSqlite() : base(
            new SqliteInMemoryDbProvider<WorkflowDbContext>()
        )
        { }
    }
}