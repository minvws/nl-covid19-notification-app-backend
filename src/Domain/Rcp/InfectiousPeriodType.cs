using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp
{
    [Flags]
    public enum InfectiousPeriodType
    {
        Asymptomatic = 0,
        Symptomatic = 1
    }
}