using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts
{
    public class ExposureContentContextFactory : IDesignTimeDbContextFactory<ExposureContentDbContext>
    {
        public ExposureContentDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ExposureContentDbContext>();
            optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=Content;Integrated Security=true");

            return new ExposureContentDbContext(optionsBuilder.Options);
        }
    }
}