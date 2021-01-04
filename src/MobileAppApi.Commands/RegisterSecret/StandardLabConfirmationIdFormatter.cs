// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.RegisterSecret
{
    public class StandardLabConfirmationIdFormatter : ILabConfirmationIdFormatter
    {
        public string Format(string value) => $"{value.Substring(0, 3)}-{value.Substring(3, 3)}";
        public string Parse(string value) => value.Replace("-", string.Empty).Substring(6);
    }
}