namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DkProcessors
{
    public enum EncodedDsosType
    {
        NotEncoded, //=> DatePrecision.Exact
        DaysSinceLastDayOfInterval, //=> DatePrecision.Range
        DaysSinceSubmissionOfKeys //=> DatePrecision.Exact

    }
}