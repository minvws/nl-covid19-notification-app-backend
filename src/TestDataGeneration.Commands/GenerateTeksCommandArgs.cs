// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.TestDataGeneration.Commands
{
    public class GenerateTeksCommandArgs
    {
        public int WorkflowCount { get; set; }
        public int TekCountPerWorkflow { get; set; }
        //public bool Authorised { get; set; }
        //public int Seed { get; set; }
    }
}