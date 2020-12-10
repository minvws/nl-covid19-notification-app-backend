// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;
using EFCore.BulkExtensions;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework
{
    public class SubsetBulkArgs
    {
        public int BatchSize { get; set; } = 10000;
        public int TimeoutSeconds { get; set; } = 6000;
        public string[] PropertiesToInclude { get; set; } = new string[0];

        public BulkConfig ToBulkConfig()
            => new BulkConfig
            {
                BatchSize = BatchSize,
                BulkCopyTimeout = TimeoutSeconds,
                UseTempDB = true,
                PropertiesToInclude = PropertiesToInclude.ToList()
            };
    }
}