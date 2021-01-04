// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Cors.Infrastructure;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet
{
    public interface ICorsOptions
    {
        public void Build(CorsPolicyBuilder options);
    }
}