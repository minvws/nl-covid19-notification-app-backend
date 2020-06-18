// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.RegisterSecret
{
    public class KeysLastSecretWriter : IKeysLastSecretWriter
    {
        private readonly WorkflowDbContext _DbContextProvider;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly RandomNumberGenerator _NumberGenerator;

        public KeysLastSecretWriter(WorkflowDbContext dbContextProvider, IUtcDateTimeProvider dateTimeProvider, RandomNumberGenerator numberGenerator)
        {
            _DbContextProvider = dbContextProvider;
            _DateTimeProvider = dateTimeProvider;
            _NumberGenerator = numberGenerator;
        }

        public async Task<EnrollmentResponse> Execute()
        {
            var entity = new KeyReleaseWorkflowState
            {
                LabConfirmationId = _NumberGenerator.GenerateToken(),
                Created = _DateTimeProvider.Now(),
                BucketId = Convert.ToBase64String(_NumberGenerator.GenerateKey()),
                ConfirmationKey = Convert.ToBase64String(_NumberGenerator.GenerateKey()),
            };

            await _DbContextProvider.KeyReleaseWorkflowStates.AddAsync(entity);

            _DbContextProvider.SaveAndCommit();

            var validDate = DateTime.Now.AddDays(1);

            return new EnrollmentResponse
            {
                ConfirmationKey = entity.ConfirmationKey,
                BucketId = entity.BucketId,
                LabConfirmationId = entity.LabConfirmationId,
                ValidUntil = new DateTime(validDate.Year, validDate.Month, validDate.Day, 4,0,0, DateTimeKind.Local)
            };
        }
    }
}