// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core
{
    public static class TableNames
    {
        public const string EksEngineInput = "EksCreateJobInput";
        public const string EksEngineOutput = "EksCreateJobOutput";
        public const string DiagnosisKeys = "DiagnosisKeys";
        public const string DiagnosisKeysInput = "DiagnosisKeysInput";

        public const string IksEngineInput = "IksCreateJobInput";
        public const string IksEngineOutput = "IksCreateJobOutput";

        public const string IncomingBatchJobs = "IncomingBatchJobs";
        public const string OutgoingBatchJobs = "OutgoingBatchJobs";

        public const string IksIn = "IksIn";
        public const string IksOut = "IksOut";
    }
}