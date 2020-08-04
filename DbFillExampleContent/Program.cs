using System;

namespace DbFillExampleContent
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            new ConsoleAppRunner().Execute(args, Configure, Start);
        }

        private static void Start(IServiceProvider services, string[] args)
        {
            services.GetRequiredService<FillDatabasesCommand>().Execute().GetAwaiter().GetResult();
        }

        private static void Configure(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddSingleton<IConfiguration>(configuration);
            services.AddLogging();
            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();

            services.AddScoped(x => DbContextStartup.Workflow(x, false));
            services.AddScoped(x => DbContextStartup.Content(x, false));

            services.AddTransient<FillDatabasesCommand>();

            services.AddTransient<WorkflowDatabaseCreateCommand>();
            services.AddTransient<ContentDatabaseCreateCommand>();

            services.AddSingleton<IWorkflowConfig, WorkflowConfig>();
            services.AddSingleton<ITekValidatorConfig, TekValidatorConfig>();
            services.AddSingleton<IEksConfig, StandardEksConfig>();

            services.AddTransient<IRandomNumberGenerator, StandardRandomNumberGenerator>();
            services.AddTransient<ILabConfirmationIdService, LabConfirmationIdService>();
            services.AddTransient<IJsonSerializer, StandardJsonSerializer>();
            services.AddTransient<IPublishingIdService, Sha256HexPublishingIdService>();
            services.AddTransient<ContentValidator>();
            services.AddTransient<ContentInsertDbCommand>();
            services.AddTransient<ZippedSignedContentFormatter>();

            services.NlSignerStartup(configuration.UseCertificatesFromResources());
        }
    }
}
