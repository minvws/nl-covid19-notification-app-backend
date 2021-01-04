using System;
using System.Collections.Generic;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain
{
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