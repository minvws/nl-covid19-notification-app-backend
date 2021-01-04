// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain
{
    public class WorkflowConfig : AppSettingsReader, IWorkflowConfig
    {
        public WorkflowConfig(IConfiguration config, string? prefix = "Workflow") : base(config, prefix)
        {
        }

        public int TimeToLiveMinutes => GetConfigValue(nameof(TimeToLiveMinutes), 1680); //24 x 60 + 4 x 60
        public int PermittedMobileDeviceClockErrorMinutes => GetConfigValue(nameof(PermittedMobileDeviceClockErrorMinutes), 30);
        public int PostKeysSignatureLength => GetConfigValue(nameof(PostKeysSignatureLength), 32);
        public int BucketIdLength => GetConfigValue(nameof(BucketIdLength), 32);
        public int ConfirmationKeyLength => GetConfigValue(nameof(ConfirmationKeyLength), 32);
        public bool CleanupDeletesData => GetConfigValue(nameof(CleanupDeletesData), false);
    }
}