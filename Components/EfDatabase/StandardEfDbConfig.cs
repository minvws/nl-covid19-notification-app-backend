// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase
{
    public class StandardEfDbConfig : IEfDbConfig
    {
        private readonly IConfiguration _Configuration;
        private readonly string _ConnStringName;

        public StandardEfDbConfig(IConfiguration configuration, string connStringName)
        {
            _Configuration = configuration;
            _ConnStringName = connStringName;
        }

        public string ConnectionString => _Configuration.GetConnectionString(_ConnStringName);
    }
}