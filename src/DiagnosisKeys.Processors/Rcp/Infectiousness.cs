// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors.Rcp
{
    public class Infectiousness : IInfectiousness
    {
        private readonly Dictionary<InfectiousPeriodType, HashSet<int>> _values;

        /// <summary>
        /// Constructor adding the the RCP values for a symptomic or asymptomic infectious period type
        /// </summary>
        /// <param name="values"></param>
        public Infectiousness(Dictionary<InfectiousPeriodType, HashSet<int>> values)
        {
            _values = values ?? throw new ArgumentNullException(nameof(values));
        }

        /// <summary>
        /// Returns if a dsos value returns to be infectious by infectious period type
        /// </summary>
        /// <param name="infectiousPeriodType">The infectious period type (A)Symptomic</param>
        /// <param name="dsos">The dsos value</param>
        /// <returns>True if infectiousness is possible, otherwise false</returns>
        public bool IsInfectious(InfectiousPeriodType infectiousPeriodType, int dsos)
        {
            return _values[infectiousPeriodType].Contains(dsos);
        }
    }
}
