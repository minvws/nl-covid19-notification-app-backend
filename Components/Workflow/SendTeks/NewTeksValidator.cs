using System;
using System.Linq;
using System.Threading;
using Microsoft.Azure.Amqp.Serialization;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks
{
    [Obsolete("Use filter approach.")]
    public class NewTeksValidator : INewTeksValidator
    {
        private readonly IGeanTekListValidationConfig _Config;
        private readonly IUtcDateTimeProvider _DateTimeProvider;

        public NewTeksValidator(IGeanTekListValidationConfig config, IUtcDateTimeProvider dateTimeProvider)
        {
            _Config = config ?? throw new ArgumentNullException(nameof(config));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        }

        public string[] Validate(Tek[] newKeys, TekReleaseWorkflowStateEntity workflow)
        {
            //30 minutes grace period cos we do not trust mobile app time.
            if (workflow.ValidUntil.AddMinutes(_Config.GracePeriodMinutes) <= _DateTimeProvider.Now())
                return new[] { "Key upload window has expired." };

            var lastExistingTekEnd = workflow.Teks.OrderBy(x => x.RollingStartNumber).LastOrDefault()?.MapToTek()?.End ?? 0;
            var firstNewTek = newKeys.OrderBy(x => x.RollingStartNumber).First();
            if (lastExistingTekEnd >= firstNewTek.RollingStartNumber)
                return new[] { "First new TEK overlaps with last, previously-existing TEK." };

            return new string[0];
        }
    }
}