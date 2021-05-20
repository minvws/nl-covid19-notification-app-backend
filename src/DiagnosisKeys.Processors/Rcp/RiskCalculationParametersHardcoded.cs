﻿using System.Collections.Generic;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors.Rcp
{
    /// <summary>
    /// Hardcoded RiskCalculationParameters for first version. This class will be replaced by <see cref="RiskCalculationParametersReader"/>
    /// </summary>
    public class RiskCalculationParametersHardcoded : IRiskCalculationParametersReader
    {
        public Dictionary<InfectiousPeriodType,  HashSet<int>> GetInfectiousDaysAsync()
        {
            var rcp = new RiskCalculationParametersSubset
            {
                InfectiousnessByDsos = new[]
                {
                    new InfectiousnessByDsosPair {Value = 0, Dsos = -14},
                    new InfectiousnessByDsosPair {Value = 0, Dsos = -13},
                    new InfectiousnessByDsosPair {Value = 0, Dsos = -12},
                    new InfectiousnessByDsosPair {Value = 0, Dsos = -11},
                    new InfectiousnessByDsosPair {Value = 0, Dsos = -10},
                    new InfectiousnessByDsosPair {Value = 0, Dsos =  -9},
                    new InfectiousnessByDsosPair {Value = 0, Dsos =  -8},
                    new InfectiousnessByDsosPair {Value = 0, Dsos =  -7},
                    new InfectiousnessByDsosPair {Value = 0, Dsos =  -6},
                    new InfectiousnessByDsosPair {Value = 0, Dsos =  -5},
                    new InfectiousnessByDsosPair {Value = 0, Dsos =  -4},
                    new InfectiousnessByDsosPair {Value = 0, Dsos =  -3},
                    new InfectiousnessByDsosPair {Value = 1, Dsos =  -2},
                    new InfectiousnessByDsosPair {Value = 1, Dsos =  -1},
                    new InfectiousnessByDsosPair {Value = 1, Dsos =   0},
                    new InfectiousnessByDsosPair {Value = 1, Dsos =   1},
                    new InfectiousnessByDsosPair {Value = 1, Dsos =   2},
                    new InfectiousnessByDsosPair {Value = 1, Dsos =   3},
                    new InfectiousnessByDsosPair {Value = 1, Dsos =   4},
                    new InfectiousnessByDsosPair {Value = 1, Dsos =   5},
                    new InfectiousnessByDsosPair {Value = 1, Dsos =   6},
                    new InfectiousnessByDsosPair {Value = 1, Dsos =   7},
                    new InfectiousnessByDsosPair {Value = 1, Dsos =   8},
                    new InfectiousnessByDsosPair {Value = 1, Dsos =   9},
                    new InfectiousnessByDsosPair {Value = 1, Dsos =  10},
                    new InfectiousnessByDsosPair {Value = 1, Dsos =  11},
                    new InfectiousnessByDsosPair {Value = 0, Dsos =  12},
                    new InfectiousnessByDsosPair {Value = 0, Dsos =  13},
                    new InfectiousnessByDsosPair {Value = 0, Dsos =  14},
                },

                 InfectiousnessByTest = new[]
                {
                    new InfectiousnessByDsosPair {Value = 0, Dsos = -14},
                    new InfectiousnessByDsosPair {Value = 0, Dsos = -13},
                    new InfectiousnessByDsosPair {Value = 0, Dsos = -12},
                    new InfectiousnessByDsosPair {Value = 0, Dsos = -11},
                    new InfectiousnessByDsosPair {Value = 0, Dsos = -10},
                    new InfectiousnessByDsosPair {Value = 0, Dsos =  -9},
                    new InfectiousnessByDsosPair {Value = 0, Dsos =  -8},
                    new InfectiousnessByDsosPair {Value = 0, Dsos =  -7},
                    new InfectiousnessByDsosPair {Value = 0, Dsos =  -6},
                    new InfectiousnessByDsosPair {Value = 0, Dsos =  -5},
                    new InfectiousnessByDsosPair {Value = 0, Dsos =  -4},
                    new InfectiousnessByDsosPair {Value = 0, Dsos =  -3},
                    new InfectiousnessByDsosPair {Value = 0, Dsos =  -2},
                    new InfectiousnessByDsosPair {Value = 0, Dsos =  -1},
                    new InfectiousnessByDsosPair {Value = 1, Dsos =   0},
                    new InfectiousnessByDsosPair {Value = 1, Dsos =   1},
                    new InfectiousnessByDsosPair {Value = 1, Dsos =   2},
                    new InfectiousnessByDsosPair {Value = 1, Dsos =   3},
                    new InfectiousnessByDsosPair {Value = 1, Dsos =   4},
                    new InfectiousnessByDsosPair {Value = 1, Dsos =   5},
                    new InfectiousnessByDsosPair {Value = 1, Dsos =   6},
                    new InfectiousnessByDsosPair {Value = 1, Dsos =   7},
                    new InfectiousnessByDsosPair {Value = 1, Dsos =   8},
                    new InfectiousnessByDsosPair {Value = 1, Dsos =   9},
                    new InfectiousnessByDsosPair {Value = 1, Dsos =  10},
                    new InfectiousnessByDsosPair {Value = 1, Dsos =  11},
                    new InfectiousnessByDsosPair {Value = 0, Dsos =  12},
                    new InfectiousnessByDsosPair {Value = 0, Dsos =  13},
                    new InfectiousnessByDsosPair {Value = 0, Dsos =  14},
                }
            };

            var dictionary = new Dictionary<InfectiousPeriodType, HashSet<int>>
            {
                {
                    InfectiousPeriodType.Symptomatic, rcp.InfectiousnessByDsos
                        .Where(x => x.Value > 0)
                        .Select(x => x.Dsos)
                        .ToHashSet()
                },
                {
                    InfectiousPeriodType.Asymptomatic, rcp.InfectiousnessByTest
                        .Where(x => x.Value > 0)
                        .Select(x => x.Dsos)
                        .ToHashSet()
                }
            };
            return dictionary;
        }
    }
}
