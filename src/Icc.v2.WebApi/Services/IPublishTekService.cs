// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.TekPublication;

namespace NL.Rijksoverheid.ExposureNotification.Icc.v2.WebApi.Services
{
    public interface IPublishTekService
    {
        Task<PublishTekResponse> ExecuteAsync(PublishTekArgs args, bool isOriginPortal);
    }
}
