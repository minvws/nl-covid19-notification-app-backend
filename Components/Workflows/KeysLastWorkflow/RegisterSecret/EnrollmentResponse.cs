using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.RegisterSecret
{
    public class EnrollmentResponse
    {
        //description: Confirmation code that the phone must display so the user can read it to an operator.
        public string LabConfirmationId { get; set; }
        //description: Random generated number that will later in the upload process associate the keys with the correct signature
        public string BucketId { get; set; }

        public string ConfirmationKey { get; set; }

        public DateTime ValidUntil { get; set; }
    }
}