// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret
{
    public class SecretWriter : ISecretWriter
    {
        private readonly WorkflowDbContext _DbContextProvider;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly RandomNumberGenerator _NumberGenerator;
        private readonly ILogger _Logger;

        public SecretWriter(WorkflowDbContext dbContextProvider, IUtcDateTimeProvider dateTimeProvider, RandomNumberGenerator numberGenerator, ILogger<SecretWriter> logger)
        {
            _DbContextProvider = dbContextProvider ?? throw new ArgumentNullException(nameof(dbContextProvider));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _NumberGenerator = numberGenerator ?? throw new ArgumentNullException(nameof(numberGenerator));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<EnrollmentResponse> Execute()
        {
            var validDate = _DateTimeProvider.Now().AddDays(1); //TODO smells like a setting

            var entity = new KeyReleaseWorkflowState
            {
                LabConfirmationId = _NumberGenerator.GenerateToken(),
                Created = _DateTimeProvider.Now(),
                BucketId = Convert.ToBase64String(_NumberGenerator.GenerateKey()),
                ConfirmationKey = Convert.ToBase64String(_NumberGenerator.GenerateKey()),
                ValidUntil = new DateTime(validDate.Year, validDate.Month, validDate.Day, 4, 0, 0, DateTimeKind.Local) //TODO smells like a setting
            };

            _Logger.LogDebug("Writing.");
            await _DbContextProvider.KeyReleaseWorkflowStates.AddAsync(entity);
            _Logger.LogDebug("Committing.");
            _DbContextProvider.SaveAndCommit();
            _Logger.LogDebug("Committed.");

            return new EnrollmentResponse
            {
                ConfirmationKey = entity.ConfirmationKey,
                BucketId = entity.BucketId,
                LabConfirmationId = $"{entity.LabConfirmationId.Substring(0, 3)}-{entity.LabConfirmationId.Substring(3, 3)}", //TODO UI concern in DB!
                Validity = (long)(entity.ValidUntil - _DateTimeProvider.Now()).TotalSeconds
            };
        }
    }
}