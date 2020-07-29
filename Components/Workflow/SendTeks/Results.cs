// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks
{
    public class Results<T>
    {
        public Results(T[] valid, string[] messages)
        {
            Items = valid ?? throw new ArgumentNullException(nameof(valid));
            Messages = messages ?? throw new ArgumentNullException(nameof(messages));
        }

        public T[] Items { get; }
        public string[] Messages { get; }
    }
}