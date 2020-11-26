// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DkProcessors;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.Interop
{
    public class TransmissionRiskLevelCalculationMk2 : ITransmissionRiskLevelCalculationMk2
    {
        private static readonly Range<int> _SignificantDayRange = new Range<int>(-2, 11);

        public Range<int> SignificantDayRange => _SignificantDayRange;

        public TransmissionRiskLevel Calculate(int daysSinceOnsetSymptoms)
        {
            //Keys before date of onset
            if (daysSinceOnsetSymptoms <= -3) return TransmissionRiskLevel.None;
            if (daysSinceOnsetSymptoms <= _SignificantDayRange.Lo) return TransmissionRiskLevel.Medium;
            if (daysSinceOnsetSymptoms <= 2) return TransmissionRiskLevel.High;
            if (daysSinceOnsetSymptoms <= 4) return TransmissionRiskLevel.Medium;
            if (daysSinceOnsetSymptoms <= _SignificantDayRange.Hi) return TransmissionRiskLevel.Low;
            return TransmissionRiskLevel.None;
            //Keys after date of onset
        }
    }

    /// <summary>
    /// For outbound NL DKs
    /// Maps actual DSOS to TRL then to the midpoint of the day range for that TRL value.
    /// TODO could replace with Range instead
    /// </summary>
    public class DosViaTrlDayRangeMidPointCalculation
    {
        private static readonly Dictionary<int, int> _Mapping = new Dictionary<int, int>
        {
            {-2, -2}, //Mid point (only)
            {-1,  0},
            { 0,  0}, //Mid point
            { 1,  0},
            { 2,  0},
            { 3,  3}, //Mid point (lower of 2)
            { 4,  3},
            { 5,  8},
            { 6,  8},
            { 7,  8},
            { 8,  8}, //Mid point
            { 9,  8},
            {10,  8},
            {11,  8}
        };

        public int Calculate(int actualDaysSinceOnsetSymptoms)
        {
            if (_Mapping.TryGetValue(actualDaysSinceOnsetSymptoms, out var result))
                return result;
            
            throw new ArgumentOutOfRangeException(nameof(actualDaysSinceOnsetSymptoms), "Trl calculation and removal of TRL.None items must precede this calculation.");
        }
    }
}