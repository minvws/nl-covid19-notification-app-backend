using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DkProcessors
{
    public static class JavascriptMaths
    {
        //JS Math.Round:
        public static int Round(double value)
        {
            return (int)Math.Floor(value + 0.5);
        }
    }
}