// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.RegisterSecret
{
    public interface IWorkflowTime
    {


        DateTime Expiry(DateTime utcNow);
        long TimeToLiveSeconds(DateTime utcNow, DateTime utcExpiry);
    }
}
