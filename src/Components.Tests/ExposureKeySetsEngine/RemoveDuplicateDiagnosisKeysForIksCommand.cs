// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using System.IO;
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
            InitSp();
        }

        [Fact]
        public async void Tests_that_no_action_taken_for_published_duplicates()
        {
            // Assemble
            using var context = _DkSourceDbProvider.CreateNew();
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, true));
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, true));
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xB }, 1, 144, false));
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xC }, 1, 144, false));
            context.SaveChanges();

            var sut = CreateCommand();

            // Act
            await sut.ExecuteAsync();

            // Assert
            Assert.Equal(2, context.DiagnosisKeys.Count(_ => _.PublishedToEfgs));
        }

        [Fact]
        public async void Tests_that_when_DK_has_been_published_that_all_duplicates_marked_as_published()
        {
            // Assemble
            using var context = _DkSourceDbProvider.CreateNew();
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, true));
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false));
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false));
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xB }, 1, 144, false));
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xC }, 1, 144, false));
            context.SaveChanges();

            var sut = CreateCommand();

            // Act
            await sut.ExecuteAsync();

            // Assert
            Assert.Equal(3, context.DiagnosisKeys.Count(_ => _.PublishedToEfgs));
        }

        [Fact]
        public async void Tests_that_when_DK_has_not_been_published_that_all_duplicates_except_the_highest_TRL_are_marked_as_published()
        {
            // Assemble
            using var context = _DkSourceDbProvider.CreateNew();
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false));
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false));
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false, TransmissionRiskLevel.High));
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xB }, 1, 144, false));
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xC }, 1, 144, false));
            context.SaveChanges();

            var sut = CreateCommand();

            // Act
            await sut.ExecuteAsync();

            // Assert
            Assert.Single(context.DiagnosisKeys.Where(x => x.PublishedToEfgs == false && x.DailyKey.KeyData == new byte[]{ 0xA }));
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


        private IRemoveDuplicateDiagnosisKeysForIksCommand CreateCommand()
        {
            return new RemoveDuplicateDiagnosisKeysForIksWithSpCommand(() => _DkSourceDbProvider.CreateNew());
        }

        private void InitSp()
        {
            var sp = File.ReadAllText(@"..\..\..\..\Database\DiagnosisKeys\dbo\StoredProcedures\RemoveDuplicateDiagnosisKeysForIks.sql");
            using var ctx = _DkSourceDbProvider.CreateNew();
            ctx.Database.ExecuteSqlRaw(sp);
        }
    }
}
