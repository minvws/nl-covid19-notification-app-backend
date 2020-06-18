// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc
{
    public interface IIccService
    {
        Task<InfectionConfirmationCodeEntity> Get(string icc);
        Task<InfectionConfirmationCodeEntity> Validate(string iccCodeString);
        Task<InfectionConfirmationCodeEntity> GenerateIcc(Guid userId, bool save = false);
        Task<List<InfectionConfirmationCodeEntity>> GenerateBatch(Guid userId, int count = 20);
        Task<InfectionConfirmationCodeEntity> RedeemIcc(string icc);
    }
}