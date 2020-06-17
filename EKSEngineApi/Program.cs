using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.KeyReleasePortalApi;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EKSEngineApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
