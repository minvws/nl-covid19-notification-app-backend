// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.Models;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ICC.Models;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ICC.Services
{
    public interface IIccService
    {
        Task<InfectionConfirmationCodeEntity> Get(string icc);
        Task<List<InfectionConfirmationCodeEntity>> GetBatchItems(string batchId);
        Task<InfectionConfirmationCodeEntity> Validate(string iccCodeString);
        Task<InfectionConfirmationCodeEntity> GenerateIcc(string userId, string batchId);
        Task<IccBatch> GenerateBatch(string userId, int count = 20);
        Task<InfectionConfirmationCodeEntity> RedeemIcc(string icc);
        Task<bool> RevokeBatch(RevokeBatchInput revokeBatchInput);
    }
}