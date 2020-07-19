using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace DbProvision
{
    class Program
    {
        private const string conn = "Data Source=.;Initial Catalog={0};Integrated Security=True";

        static async Task Main(string[] args)
        {
            await new ContentDbContext(new SqlServerDbContextOptionsBuilder(string.Format(conn, "content")).Build()).Database.EnsureCreatedAsync();
            await new WorkflowDbContext(new SqlServerDbContextOptionsBuilder(string.Format(conn, "workflow")).Build()).Database.EnsureCreatedAsync();
            await new IccBackendContentDbContext(new SqlServerDbContextOptionsBuilder(string.Format(conn, "icc")).Build()).Database.EnsureCreatedAsync();
        }
    }
}
