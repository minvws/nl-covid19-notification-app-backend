using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks
{
    [Obsolete("Use filter approach.")]
    public interface INewTeksValidator 
    {
        string[] Validate(Tek[] newKeys, TekReleaseWorkflowStateEntity workflow);
    }
}