// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

//using System;
//using System.Linq;
//using System.Threading;
//using Microsoft.Azure.Amqp.Serialization;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens;

//namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks
//{
//    [Obsolete("Use filter approach.")]
//    public class NewTeksValidator : INewTeksValidator
//    {
//        private readonly ITekListValidationConfig _Config;
//        private readonly IUtcDateTimeProvider _DateTimeProvider;

//        public NewTeksValidator(ITekListValidationConfig config, IUtcDateTimeProvider dateTimeProvider)
//        {
//            _Config = config ?? throw new ArgumentNullException(nameof(config));
//            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
//        }

//        public string[] Validate(Tek[] newKeys, TekReleaseWorkflowStateEntity workflow)
//        {
//            //xx minutes grace period cos we do not trust mobile app time. Now baked into ValidUntil.
//            if (workflow.ValidUntil <= _DateTimeProvider.Now())
//                return new[] { "Key upload window has expired." };

//            var lastExistingTekEnd = workflow.Teks.OrderBy(x => x.RollingStartNumber).LastOrDefault()?.MapToTek()?.End ?? 0;
//            var firstNewTek = newKeys.OrderBy(x => x.RollingStartNumber).First();

//            if (lastExistingTekEnd >= firstNewTek.RollingStartNumber)
//                return new[] { "First new TEK overlaps with last, previously-existing TEK." };

//            return new string[0];
//        }
//    }
//}