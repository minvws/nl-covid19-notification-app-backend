// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Iks.Protobuf;

namespace EfgsTestDataGenerator.Models
{
    public class EfgsDataSet
    {
        public string BatchTag { get; set; }
        public string NextBatchTag { get; set; }
        public DiagnosisKeyBatch Content { get; set; }
    }
}
