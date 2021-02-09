// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Config
{
    public interface IIccIdentityHubConfig
    {
        string BaseUrl { get; }
        string Tenant { get; }
        string ClientId { get; }
        string ClientSecret { get; }
        string CallbackPath { get; }
    }

    public class DefaultProductionValuesIccIdentityHubConfig : IIccIdentityHubConfig
    {
        public string BaseUrl => throw new MissingConfigurationValueException(nameof(BaseUrl));

        public string Tenant => throw new MissingConfigurationValueException(nameof(Tenant));

        public string ClientId => throw new MissingConfigurationValueException(nameof(ClientId));

        public string ClientSecret => throw new MissingConfigurationValueException(nameof(ClientSecret));

        public string CallbackPath => throw new MissingConfigurationValueException(nameof(CallbackPath));
    }
}