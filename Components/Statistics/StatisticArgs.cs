using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Statistics
{
    //public class IncrementingCounterArgs
    //{
    //    public string Name { get; set; }
    //    public string Qualifier { get; set; }
    //}

    //public interface IStatisticsCounterWriter
    //{    
    //    void IncrementCounter(IncrementingCounterArgs args);
    //}

    public class StatisticArgs
    {
        public string Name { get; set; }
        public string Qualifier { get; set; }

        /// <summary>
        /// Or are they all ints?
        /// </summary>
        public double Value { get; set; }
    }
}
