// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
    public class RemoveLocalDuplicateDiagnosisKeysCommandTests : IDisposable
    {
        private readonly IDbProvider<DkSourceDbContext> _dkSourceDbProvider;

        public RemoveLocalDuplicateDiagnosisKeysCommandTests()
        {
            _dkSourceDbProvider = new SqlServerDbProvider<DkSourceDbContext>(nameof(RemoveDuplicateDiagnosisKeysForIksCommandTests) + "_DK");
            var sp = File.ReadAllText(Path.Combine(Path.GetDirectoryName(NCrunch.Framework.NCrunchEnvironment.GetOriginalSolutionPath()), @"Database\DiagnosisKeys\dbo\StoredProcedures\RemoveLocalDuplicateDiagnosisKeys.sql"));
            using var ctx = _dkSourceDbProvider.CreateNew();
            ctx.Database.ExecuteSqlRaw(sp);
        }

        [Fact]
        [ExclusivelyUses(nameof(RemoveLocalDuplicateDiagnosisKeysCommandTests))]
        public async Task No_Action_Taken_For_Published_Duplicates()
        {

            // Assemble
            using var context = _dkSourceDbProvider.CreateNew();
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, true));
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, true));
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xB }, 1, 144, false));
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xC }, 1, 144, false));
            context.SaveChanges();

            var sut = CreateCommand();

            // Act
            await sut.ExecuteAsync();

            // Assert
            Assert.Equal(2, context.DiagnosisKeys.Count(_ => _.PublishedLocally));
        }

        [Fact]
        [ExclusivelyUses(nameof(RemoveLocalDuplicateDiagnosisKeysCommandTests))]
        public async void When_DK_Has_Been_Published_All_Duplicates_Marked_As_Published()
        {
            // Assemble
            using var context = _dkSourceDbProvider.CreateNew();
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
            Assert.Equal(3, context.DiagnosisKeys.Count(_ => _.PublishedLocally));
        }

        [Fact]
        [ExclusivelyUses(nameof(RemoveLocalDuplicateDiagnosisKeysCommandTests))]
        public async Task When_DK_Has_Not_Been_Published_All_Duplicates_Except_The_Highest_TRL_Are_Marked_As_Published()
        {
            // Assemble
            await using var context = _dkSourceDbProvider.CreateNew();
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false));
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false));
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false, null, TransmissionRiskLevel.High));
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xB }, 1, 144, false));
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xC }, 1, 144, false));
            await context.SaveChangesAsync();

            var sut = CreateCommand();

            // Act
            await sut.ExecuteAsync();

            // Assert
            Assert.Single(context.DiagnosisKeys.Where(x => x.Local.TransmissionRiskLevel == TransmissionRiskLevel.High && x.PublishedLocally == false && x.DailyKey.KeyData == new byte[] { 0xA }));
            Assert.Equal(2, context.DiagnosisKeys.Count(x => x.Local.TransmissionRiskLevel == TransmissionRiskLevel.Low && x.PublishedLocally == true && x.DailyKey.KeyData == new byte[] { 0xA }));
        }

        [Fact]
        [ExclusivelyUses(nameof(RemoveLocalDuplicateDiagnosisKeysCommandTests))]
        public async Task When_DK_Has_Not_Been_Published_All_Duplicates_Except_The_First_Confirmed_Are_Marked_As_Published()
        {
            // Assemble
            var firstCreatedDate = DateTime.Now.AddHours(-1);
            var otherCreatedDate = DateTime.Now;

            await using var context = _dkSourceDbProvider.CreateNew();
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false, firstCreatedDate));
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false, otherCreatedDate));
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false, otherCreatedDate));
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xB }, 1, 144, false, otherCreatedDate));
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xC }, 1, 144, false, otherCreatedDate));
            await context.SaveChangesAsync();

            var sut = CreateCommand();

            // Act
            await sut.ExecuteAsync();

            // Assert
            // For Keydata '0xA', expected a single entity having the first Created date and Not published locally
            Assert.Single(context.DiagnosisKeys.Where(x => x.Created == firstCreatedDate && x.PublishedLocally == false && x.DailyKey.KeyData == new byte[] { 0xA }));
            // For Keydata '0xA', expected 2 entities having a later Created date and both published locally
            Assert.Equal(2, context.DiagnosisKeys.Count(x => x.Created == otherCreatedDate && x.PublishedLocally == true && x.DailyKey.KeyData == new byte[] { 0xA }));
        }

        private static DiagnosisKeyEntity CreateDk(byte[] keyData, int rsn, int rp, bool publishedLocally, DateTime? created = null, TransmissionRiskLevel trl = TransmissionRiskLevel.Low)
        {
            return new DiagnosisKeyEntity
            {
                DailyKey = new DailyKey
                {
                    KeyData = keyData,
                    RollingPeriod = rp,
                    RollingStartNumber = rsn
                },
                PublishedToEfgs = true,
                PublishedLocally = publishedLocally,
                Local = new LocalTekInfo
                {
                    TransmissionRiskLevel = trl
                },
                Created = created ?? DateTime.Now
            };
        }

        private IRemoveDuplicateDiagnosisKeysCommand CreateCommand()
        {
            return new RemoveLocalDuplicateDiagnosisKeysCommand(() => _dkSourceDbProvider.CreateNew());
        }

        public void Dispose()
        {
            _dkSourceDbProvider.Dispose();
        }
    }
}
