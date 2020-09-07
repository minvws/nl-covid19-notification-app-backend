// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Http;
using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public interface IContentExpiryStrategy
    {
        ExpiryStatus Apply(DateTime created, IHeaderDictionary headers);
    }
}