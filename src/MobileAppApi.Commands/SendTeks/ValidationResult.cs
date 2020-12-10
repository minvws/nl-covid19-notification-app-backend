// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.SendTeks
{
    public class ValidationResult<T> where T : class
    {
        public ValidationResult(T valid)
        {
            Item = valid;
            Messages = new string[0];
        }

        public ValidationResult(string[] messages)
        {
            Item = null;
            Messages = messages ?? throw new ArgumentNullException(nameof(messages));
        }

        public bool Valid => Item != null;
        public T Item { get; }
        public string[] Messages { get; }
    }
}