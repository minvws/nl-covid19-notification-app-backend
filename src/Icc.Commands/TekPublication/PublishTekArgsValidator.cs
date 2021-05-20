// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.LuhnModN;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.TekPublication
{
    public class PublishTekArgsValidator
    {
        private readonly ILuhnModNValidator _luhnModNValidator;
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        
        public PublishTekArgsValidator(ILuhnModNValidator luhnModNValidator, IUtcDateTimeProvider dateTimeProvider)
        {
            _luhnModNValidator = luhnModNValidator ?? throw new ArgumentNullException(nameof(luhnModNValidator));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        }

        public string[] Validate(PublishTekArgs args)
        {
            if (args == null)
            {
                return new[] {"Args is null."};
            }

            var errors = new List<string>();

            // The PubTEK key cannot be empty
            if (string.IsNullOrWhiteSpace(args.GGDKey))
            {
                return new[] {"PubTEK key is null or empty."};
            }

            // The PubTEK key should be 6 or 7 characters in length
            if (args.GGDKey.Length != _luhnModNValidator.Config.ValueLength && args.GGDKey.Length != _luhnModNValidator.Config.ValueLength - 1)
            {
                return new[] { "PubTEK key has incorrect length." };
            }
            
            // The PubTEK key should only have character from the given set
            if (args.GGDKey.Any(x => !_luhnModNValidator.Config.CharacterSet.Contains(x)))
            {
                return new[] { "PubTEK key contains invalid character." };
            }

            // The PubTEK key should be validated for LuhnModN algorithm when it has 7 characters
            if (args.GGDKey.Length == _luhnModNValidator.Config.ValueLength && !_luhnModNValidator.Validate(args.GGDKey))
            {
                return new[] { "PubTEK key validation for LuhnModN failed." };
            }

            //Should be a date only without time.
            args.SelectedDate = args.SelectedDate?.Date;
            if (!args.Symptomatic && (_dateTimeProvider.Snapshot.Date.AddDays(-30) > args.SelectedDate?.Date || args.SelectedDate?.Date > _dateTimeProvider.Snapshot.Date))
            {
                errors.Add($"Selected date out of range - {args.SelectedDate}.");
            }

            return errors.ToArray();
        }
    }
}