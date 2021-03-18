using System;
using System.Collections.Generic;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors.Rcp
{ public class DsosInfectiousness : IDsosInfectiousness
    {
        private readonly HashSet<int> _Values;

        public DsosInfectiousness(HashSet<int> values)
        {
            _Values = values ?? throw new ArgumentNullException(nameof(values));
        }

        public bool IsInfectious(int dsos) => _Values.Contains(dsos);
    }
}
