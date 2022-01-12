// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EfgsUploader.Jobs
{
    public class IksUploadBatchJob : IJob
    {
        private readonly ILogger _logger;
        private readonly IEfgsConfig _efgsConfig;
        private readonly CommandInvoker _commandInvoker;
        private readonly IksSendBatchCommand _iksSendBatchCommand;

        public IksUploadBatchJob(
            ILogger<IksUploadBatchJob> logger,
            IEfgsConfig efgsConfig,
            CommandInvoker commandInvoker,
            IksSendBatchCommand iksSendBatchCommand
        )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _efgsConfig = efgsConfig ?? throw new ArgumentNullException(nameof(efgsConfig));
            _commandInvoker = commandInvoker ?? throw new ArgumentNullException(nameof(commandInvoker));
            _iksSendBatchCommand = iksSendBatchCommand ?? throw new ArgumentNullException(nameof(iksSendBatchCommand));
        }

        public void Run()
        {
            if (!_efgsConfig.UploaderEnabled)
            {
                _logger.LogWarning("EfgsUploader is disabled by the configuration.");
                return;
            }

            _commandInvoker
                .SetCommand(_iksSendBatchCommand)
                .Execute();
        }
    }
}
