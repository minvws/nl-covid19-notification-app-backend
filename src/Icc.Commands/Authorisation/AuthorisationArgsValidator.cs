// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation
{
    public class AuthorisationArgsValidator
    {
        private readonly ILabConfirmationIdService _labConfirmationIdService;
        private readonly IUtcDateTimeProvider _dateTimeProvider;

        public AuthorisationArgsValidator(ILabConfirmationIdService labConfirmationIdService, IUtcDateTimeProvider dateTimeProvider)
        {
            _labConfirmationIdService = labConfirmationIdService ?? throw new ArgumentNullException(nameof(labConfirmationIdService));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        }

        public string[] Validate(AuthorisationArgs args)
        {
            if (args == null)
            {
                return new[] { "Args is null." };
            }

            //Should be a date.
            args.DateOfSymptomsOnset = args.DateOfSymptomsOnset.Date;

            var errors = new List<string>();
            errors.AddRange(_labConfirmationIdService.Validate(args.LabConfirmationId));

            if (_dateTimeProvider.Snapshot.Date.AddDays(-30) > args.DateOfSymptomsOnset.Date || args.DateOfSymptomsOnset.Date > _dateTimeProvider.Snapshot.Date)
            {
                errors.Add($"Date of symptoms onset out of range - {args.DateOfSymptomsOnset}.");
            }

            return errors.ToArray();
        }

    }
}