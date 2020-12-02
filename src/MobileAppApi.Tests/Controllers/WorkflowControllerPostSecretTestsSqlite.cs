using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace MobileAppApi.Tests.Controllers
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