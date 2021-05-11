using System;
using System.Collections.Generic;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors.Rcp
{ public class Infectiousness : IInfectiousness
    {
        private readonly Dictionary<InfectiousPeriodType, HashSet<int>> _Values;

        public Infectiousness(Dictionary<InfectiousPeriodType, HashSet<int>> values)
        {
            _Values = values ?? throw new ArgumentNullException(nameof(values));
        }

        public bool IsInfectious(InfectiousPeriodType infectiousPeriodType, int dsos)
        {

            return _Values[infectiousPeriodType].Contains(dsos);
        }
    }
}
