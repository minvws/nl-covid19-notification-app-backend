// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework
{
    public class StandardEfDbConfig : IEfDbConfig
    {
        private readonly IConfiguration _Configuration;
        private readonly string _ConnStringName;

        public StandardEfDbConfig(IConfiguration configuration, string connStringName)
        {
            _Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            if (string.IsNullOrWhiteSpace(connStringName)) throw new ArgumentException(nameof(connStringName));
            _ConnStringName = connStringName;
        }

        public string ConnectionString
        {
            get { 
                var result = _Configuration.GetConnectionString(_ConnStringName);

                if (string.IsNullOrWhiteSpace(result))
                    throw new MissingConfigurationValueException(_ConnStringName);

                return result;
            }
        }
    }
}