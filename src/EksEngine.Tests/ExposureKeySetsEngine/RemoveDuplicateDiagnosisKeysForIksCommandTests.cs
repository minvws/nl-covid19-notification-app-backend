// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NCrunch.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Tests.ExposureKeySetsEngine
{
    [Trait("db", "ss")]
    public class RemoveDuplicateDiagnosisKeysForIksCommandTests:IDisposable
    {
        private readonly IDbProvider<DkSourceDbContext> _DkSourceDbProvider;

        public RemoveDuplicateDiagnosisKeysForIksCommandTests()
        {
            _DkSourceDbProvider = new SqlServerDbProvider<DkSourceDbContext>(nameof(RemoveDuplicateDiagnosisKeysForIksCommandTests)+"_DK");
            var sp = File.ReadAllText(Path.Combine(Path.GetDirectoryName(NCrunch.Framework.NCrunchEnvironment.GetOriginalSolutionPath()), @"Database\DiagnosisKeys\dbo\StoredProcedures\RemoveDuplicateDiagnosisKeysForIks.sql"));
            using var ctx = _DkSourceDbProvider.CreateNew();
            ctx.Database.ExecuteSqlRaw(sp);
        }

        [Fact]
        [ExclusivelyUses(nameof(RemoveDuplicateDiagnosisKeysForIksCommandTests))]
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
        [ExclusivelyUses(nameof(RemoveDuplicateDiagnosisKeysForIksCommandTests))]
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
        [ExclusivelyUses(nameof(RemoveDuplicateDiagnosisKeysForIksCommandTests))]
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

        public void Dispose()
        {
            _DkSourceDbProvider.Dispose();
        }
    }
}
