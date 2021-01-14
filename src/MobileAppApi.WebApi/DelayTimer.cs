using System.Diagnostics;
using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi
{
    public static class DelayTimer
    {
        public static async Task Delay(int milliseconds)
        {
            var timer = new Stopwatch();

            timer.Start();
            while (timer.ElapsedMilliseconds <= milliseconds)
            {
                // just sit and wait
            }

            timer.Stop();
        }
    }
}
