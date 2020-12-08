//// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
//// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
//// SPDX-License-Identifier: EUPL-1.2

//using System.Linq;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DkProcessors;

//namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.IksInbound
//{
//    // TODO has Steve renamed this?
//    public static class ProcessorExecutionExtension
//    {
//        public static IksImportItem?[] Execute(this IDiagnosticKeyProcessor[] processors, IksImportItem?[] items)
//        {
//            var result = items.ToArray();
//            foreach (var i in processors)
//            {
//                result = result
//                    .Select(i.Execute) //Apply filter
//                    .Where(x => x != null) //Remove excluded items
//                    .ToArray();
//            }
//            return result;
//        }
//    }
//}