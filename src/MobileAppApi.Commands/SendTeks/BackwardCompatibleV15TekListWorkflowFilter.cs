// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.SendTeks
{
    public class BackwardCompatibleV15TekListWorkflowFilter : ITekListWorkflowFilter
    {
        private TekReleaseWorkflowStateEntity _Workflow;
        private bool _WorkflowHasKeyOnCreationDate;
        private List<Tek> _Valid;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private int? _LastPublishedRsn;
        private readonly ITekValidatorConfig _Config;

        public BackwardCompatibleV15TekListWorkflowFilter(IUtcDateTimeProvider dateTimeProvider, ITekValidatorConfig config)
        {
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _Config = config ?? throw new ArgumentException(nameof(config));
        }

        public FilterResult<Tek> Filter(Tek[] newKeys, TekReleaseWorkflowStateEntity workflow)
        {
            _Workflow = workflow;
            _WorkflowHasKeyOnCreationDate = _Workflow.Teks.Any(x => x.RollingStartNumber.FromRollingStartNumber().Date == _Workflow.Created.Date);

            _LastPublishedRsn = workflow.Teks
                .Where(x => x.PublishingState == PublishingState.Published)
                .OrderByDescending(x => x.RollingStartNumber)
                .FirstOrDefault()?.RollingStartNumber;


            _Valid = new List<Tek>(newKeys.Length);

            var messages = newKeys.SelectMany(x => ValidateSingleTek(x).Select(y => $"{y} - RSN:{x.RollingStartNumber} KeyData:{Convert.ToBase64String(x.KeyData)}")).ToArray();

            return new FilterResult<Tek>(_Valid.ToArray(), messages);
        }

        private string[] ValidateSingleTek(Tek item)
        {
            if (_LastPublishedRsn.HasValue && item.RollingStartNumber <= _LastPublishedRsn)
            {
                return new[] { "Before last published." };
            }

            if (item.StartDate > _Workflow.Created.Date)
            {
                //Kill 'same day' keys
                return new[] { "Too recent." };
            }

            var snapshot = _DateTimeProvider.Snapshot;
            if (item.StartDate == snapshot.Date)
            {
                // this is a ‘same day key’, generated on the day the user gets result. 
                // it must arrive within the call window (Step 4 from proposed process)
                if (_Workflow.AuthorisedByCaregiver != null && !(snapshot - _Workflow.AuthorisedByCaregiver < TimeSpan.FromMinutes(_Config.AuthorisationWindowMinutes)))
                {
                    return new[] {"Authorisation window expired."};
                }

                item.PublishAfter = snapshot.AddMinutes(_Config.PublishingDelayInMinutes);
                _Valid.Add(item);
                return new [] { "Same day key accepted - Reason:Before call or within call window Key." };
            }

            // key from result day that arrives after today with extra check to ensure it’s a 1.4 device and not a tampered 1.5 device (Step 5)
            if (item.StartDate == _Workflow.Created.Date && _WorkflowHasKeyOnCreationDate)
            {
                return new[] { "Tek for result day after midnight rejected - already a tek present for that day." };
            }

            _Valid.Add(item);
            return new string[0];
        }
    }
}