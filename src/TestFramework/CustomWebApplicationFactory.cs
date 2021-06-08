// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework
{
    public class CustomWebApplicationFactory<TStartup, TContext> : WebApplicationFactory<TStartup> where TStartup : class where TContext : DbContext, IDisposable
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the app's ApplicationDbContext registration.
                var descriptor = services.SingleOrDefault(d => d.ServiceType.Name == typeof(TContext).Name);

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add ApplicationDbContext using an in-memory database for testing.
                services.AddDbContext<TContext>((options, context) =>
                {
                    context.UseInMemoryDatabase("InMemoryDbForTesting");
                });
            });
        }
    }
}
