using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine
{
    public interface ITransmissionRiskLevelCalculation
    {
        TransmissionRiskLevel Calculate(int tekRollingPeriodNumber, DateTime dateOfSymptomsOnset);
    }
}