using System;
using Microsoft.Extensions.DependencyInjection;
using TheIdentityHub.AspNetCore.Authentication;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomOptions(this IServiceCollection serviceCollection,
        string name, Action<TheIdentityHubOptions> options)
    {
        serviceCollection.Configure(name, options);
        return serviceCollection;
    }
}