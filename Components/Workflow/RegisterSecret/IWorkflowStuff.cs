using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret
{
    public interface IWorkflowStuff
    {
        DateTime Expiry(DateTime utcNow);
        long TimeToLiveSeconds(DateTime utcNow, DateTime utcExpiry);
    }
}