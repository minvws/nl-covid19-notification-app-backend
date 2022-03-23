// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound
{
    public class BatchTagProvider : IBatchTagProvider
    {
        private readonly Sha256HashService _imp = new Sha256HashService();

        public string Create(byte[] content)
        {
            return _imp.Create(content);
        }
    }
}
