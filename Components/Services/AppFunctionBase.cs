using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;

namespace CdnRegionSync
{
    public abstract class AppFunctionBase
    {
        protected IConfigurationRoot Configuration { get; private set; }

        protected void SetConfig(ExecutionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (Configuration != null)
                return;

            Configuration = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}