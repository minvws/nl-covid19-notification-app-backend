// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using System;
using System.Linq;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.ExposureKeySetsEngine
{
    public class RemoveDuplicateDiagnosisKeysForIksCommandTests
    {
        private readonly IDbProvider<DkSourceDbContext> _DkSourceDbProvider;

        public RemoveDuplicateDiagnosisKeysForIksCommandTests()
        {
            _DkSourceDbProvider = new SqlServerDbProvider<DkSourceDbContext>(nameof(RemoveDuplicateDiagnosisKeysForIksCommandTests));
        }

        [Fact]
        public async void Tests_that_no_action_taken_for_published_duplicates()
        {
            // Assemble
            Func<DkSourceDbContext> factory = () => _DkSourceDbProvider.CreateNew();
            using var context = factory();
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, true));
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, true));
            context.SaveChanges();

            var sut = new RemoveDuplicateDiagnosisKeysForIksCommand(factory);

            // Act
            await sut.ExecuteAsync();

            // Assert
            Assert.DoesNotContain(context.DiagnosisKeys, dk => !dk.PublishedToEfgs);
        }

        [Fact]
        public async void Tests_that_when_DK_has_been_published_that_all_duplicates_marked_as_published()
        {
            // Assemble
            Func<DkSourceDbContext> factory = () => _DkSourceDbProvider.CreateNew();
            using var context = factory();
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, true));
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false));
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false));
            context.SaveChanges();

            var sut = new RemoveDuplicateDiagnosisKeysForIksCommand(factory);

            // Act
            await sut.ExecuteAsync();

            // Assert
            Assert.DoesNotContain(context.DiagnosisKeys, dk => !dk.PublishedToEfgs);
        }

        [Fact]
        public async void Tests_that_when_DK_has_not_been_published_that_all_duplicates_except_the_highest_TRL_are_marked_as_published()
        {
            // Assemble
            Func<DkSourceDbContext> factory = () => _DkSourceDbProvider.CreateNew();
            using var assembleContext = factory();
            assembleContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false, TransmissionRiskLevel.Low));
            assembleContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false, TransmissionRiskLevel.Low));
            assembleContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false, TransmissionRiskLevel.High));
            assembleContext.SaveChanges();

            var sut = new RemoveDuplicateDiagnosisKeysForIksCommand(factory);

            // Act
            await sut.ExecuteAsync();

            // Assert
            using var resultContext = factory();
            var all = resultContext.DiagnosisKeys.ToList();
            Assert.Single(resultContext.DiagnosisKeys.Where(x => x.PublishedToEfgs == false));
        }

        private static DiagnosisKeyEntity CreateDk(byte[] keyData, int rsn, int rp, bool pEfgs, TransmissionRiskLevel trl = TransmissionRiskLevel.Low)
        {
            return new DiagnosisKeyEntity
            {
                DailyKey = new DailyKey
                {
                    KeyData = keyData,
                    RollingPeriod = rp,
                    RollingStartNumber = rsn
                },
                PublishedToEfgs = pEfgs,
                PublishedLocally = true,
                Local = new LocalTekInfo
                {
                    TransmissionRiskLevel = trl
                }
            };
        }

    }
}
