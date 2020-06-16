using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using Swashbuckle.AspNetCore.SwaggerGen;


namespace NL.Rijksoverheid.ExposureNotification.ICCBackend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(options => { options.RespectBrowserAcceptHeader = true; });

            services.AddControllers();

            // Database Scoping
            services.AddScoped(x =>
            {
                var config = new StandardEfDbConfig(Configuration, "ICC");
                var builder = new SqlServerDbContextOptionsBuilder(config);
                var result = new ICCBackendContentDbContext(builder.Build());
                return result;
            });
            services.AddScoped<IEfDbConfig>(x => new StandardEfDbConfig(Configuration, "ICC"));
            services.AddScoped<ProvisionDatabasesCommandICC, ProvisionDatabasesCommandICC>();
            services.AddScoped<IUtcDateTimeProvider, StandardUtcDateTimeProvider>();


            services.AddScoped<IICCService, ICCService>();
            services.AddScoped<AppBackendService, AppBackendService>();
            services.AddAuthentication("ICCAuthentication")
                .AddScheme<AuthenticationSchemeOptions, ICCAuthenticationHandler>("ICCAuthentication", null);


            services.AddCors();

            services.AddSwaggerGen(o =>
            {
                o.SwaggerDoc("v1", new OpenApiInfo {Title = "ICC Back-end Server", Version = "v1"});
                o.AddSecurityDefinition("ICC", new OpenApiSecurityScheme
                {
                    Description = "ICC Code Authentication",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "ICCAuthentication"
                });
                o.OperationFilter<SecurityRequirementsOperationFilter>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors(options => options.AllowAnyOrigin().AllowAnyHeader());
            
            app.UseSwagger();
            app.UseSwaggerUI(o => { o.SwaggerEndpoint("/swagger/v1/swagger.json", "ICC Back-end Server V1"); });


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }

    public class SecurityRequirementsOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Policy names map to scopes
            var requiredScopes = context.MethodInfo
                .GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .Select(attr => attr.Policy)
                .Distinct();

            if (requiredScopes.Any())
            {
                operation.Responses.Add("401", new OpenApiResponse {Description = "Unauthorized"});
                operation.Responses.Add("403", new OpenApiResponse {Description = "Forbidden"});

                var ICCAuthenticationScheme = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id = "ICC"}
                };

                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        [ICCAuthenticationScheme] = requiredScopes.ToList()
                    }
                };
            }
        }
    }
}