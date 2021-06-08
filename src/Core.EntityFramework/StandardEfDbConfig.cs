// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework
{
    public class StandardEfDbConfig : IEfDbConfig
    {
        private readonly IConfiguration _configuration;
        private readonly string _connStringName;

        public StandardEfDbConfig(IConfiguration configuration, string connStringName)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            if (string.IsNullOrWhiteSpace(connStringName))
            {
                throw new ArgumentException(nameof(connStringName));
            }

            _connStringName = connStringName;
        }

        public string ConnectionString
        {
            get
            {
                var result = _configuration.GetConnectionString(_connStringName);

                if (string.IsNullOrWhiteSpace(result))
                {
                    throw new MissingConfigurationValueException(_connStringName);
                }

                return result;
            }
        }
    }
}