namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.DecoyKeys
{
    public class WelfordsAlgorithmState
    {
        public WelfordsAlgorithmState(int count, double mean, double standardDeviation)
        {
            Count = count;
            Mean = mean;
            StandardDeviation = standardDeviation;
        }

        public int Count { get; }
        public double Mean { get; }
        public double StandardDeviation { get; }
    }
}