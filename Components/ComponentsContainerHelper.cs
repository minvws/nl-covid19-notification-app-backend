// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.DependencyInjection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components
{
    /// <summary>
    /// Helper class to register all of the standard/common components with the IoC container.
    /// </summary>
    public static class ComponentsContainerHelper
    {
        /// <summary>
        /// Register default implementations for the injectable services in the Components assembly with the IoC container
        /// </summary>
        /// <param name="services"></param>
        public static void RegisterDefaultServices(IServiceCollection services)
        {
            services.AddScoped<IJsonSerializer, StandardJsonSerializer>();
        }
    }
}
