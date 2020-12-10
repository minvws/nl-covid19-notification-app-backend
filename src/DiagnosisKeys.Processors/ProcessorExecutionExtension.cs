// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors
{
    public static class ProcessorExecutionExtension
    {
        public static DkProcessingItem?[] Execute(this IDiagnosticKeyProcessor[] processors, DkProcessingItem?[] items)
        {
            var result = items.ToArray();
            foreach (var i in processors)
            {
                result = result
                    .Select(i.Execute) //Apply filter
                    .Where(x => x != null) //Remove excluded items
                    .ToArray();
            }
            return result;
        }
    }
}