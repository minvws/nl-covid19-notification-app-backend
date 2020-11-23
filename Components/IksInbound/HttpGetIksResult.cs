// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Net;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.IksInbound
{
    public class HttpGetIksResult
    {
        public EfgsDownloadResponseCode ResponseCode { get; set; }
        public HttpGetIksSuccessResult? SuccessInfo { get; set; }
    }

    public enum EfgsDownloadResponseCode
    {
        Ok,
        NoDataFound
    }
}