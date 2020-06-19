// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Generic;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc;

namespace NL.Rijksoverheid.ExposureNotification.IccBackend.Models
{
    public class IccBatch
    {
        public string BatchIdentifier;
        public List<InfectionConfirmationCodeEntity> Batch { get; set; }

        public IccBatch(string batchIdentifier, List<InfectionConfirmationCodeEntity> batch)
        {
            BatchIdentifier = batchIdentifier;
            Batch = batch;
        }

    }
}