// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.DiagnosisKeys.Commands;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Tests.ExposureKeySetsEngine
{
    [Trait("db", "mem")]
    public class RemoveDuplicateDiagnosisKeysCommandTests : IDisposable
    {
        private static DbConnection connection;
        private readonly DiagnosisKeysDbContext _diagnosisKeysContext;

        public RemoveDuplicateDiagnosisKeysCommandTests()
        {
            var diagnosisKeysDbContextOptions = new DbContextOptionsBuilder<DiagnosisKeysDbContext>().UseSqlite(CreateSqlDatabase()).Options;
            _diagnosisKeysContext = new DiagnosisKeysDbContext(diagnosisKeysDbContextOptions);
            _diagnosisKeysContext.Database.EnsureCreated();
        }

        private static DbConnection CreateSqlDatabase()
        {
            connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            return connection;
        }

        public void Dispose() => connection.Dispose();

        [Fact]
        public async Task No_Action_Taken_For_Published_Duplicates()
        {
            // Arrange
            await _diagnosisKeysContext.BulkDeleteAsync(_diagnosisKeysContext.DiagnosisKeys.ToList());

            _diagnosisKeysContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, true));
            _diagnosisKeysContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, true));
            _diagnosisKeysContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xB }, 1, 144, false));
            _diagnosisKeysContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xC }, 1, 144, false));
            await _diagnosisKeysContext.SaveChangesAsync();

            var sut = CreateCommand();

            // Act
            await sut.ExecuteAsync();

            // Assert
            Assert.Equal(2, _diagnosisKeysContext.DiagnosisKeys.Count(_ => _.PublishedLocally));
        }

        [Fact]
        public async void When_DK_Has_Been_Published_All_Duplicates_Marked_As_Published()
        {
            // Arrange
            await _diagnosisKeysContext.BulkDeleteAsync(_diagnosisKeysContext.DiagnosisKeys.ToList());

            _diagnosisKeysContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, true));
            _diagnosisKeysContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false));
            _diagnosisKeysContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false));
            _diagnosisKeysContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xB }, 1, 144, false));
            _diagnosisKeysContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xC }, 1, 144, false));
            await _diagnosisKeysContext.SaveChangesAsync();

            var sut = CreateCommand();

            // Act
            await sut.ExecuteAsync();

            // Assert
            Assert.Equal(3, _diagnosisKeysContext.DiagnosisKeys.Count(_ => _.PublishedLocally));
        }

        [Fact]
        public async Task When_DK_Has_Not_Been_Published_All_Duplicates_Except_The_Highest_TRL_Are_Marked_As_Published()
        {
            // Arrange
            await _diagnosisKeysContext.BulkDeleteAsync(_diagnosisKeysContext.DiagnosisKeys.ToList());

            _diagnosisKeysContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false));
            _diagnosisKeysContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false));
            _diagnosisKeysContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false, null, TransmissionRiskLevel.High));
            _diagnosisKeysContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xB }, 1, 144, false));
            _diagnosisKeysContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xC }, 1, 144, false));
            await _diagnosisKeysContext.SaveChangesAsync();

            var sut = CreateCommand();

            // Act
            await sut.ExecuteAsync();

            // Assert
            Assert.Single(_diagnosisKeysContext.DiagnosisKeys.Where(x => x.Local.TransmissionRiskLevel == TransmissionRiskLevel.High && x.PublishedLocally == false && x.DailyKey.KeyData == new byte[] { 0xA }));
            Assert.Equal(2, _diagnosisKeysContext.DiagnosisKeys.Count(x => x.Local.TransmissionRiskLevel == TransmissionRiskLevel.Low && x.PublishedLocally == true && x.DailyKey.KeyData == new byte[] { 0xA }));
        }

        [Fact]
        public async Task When_DK_Has_Not_Been_Published_All_Duplicates_Except_The_First_Confirmed_Are_Marked_As_Published()
        {
            // Assemble
            var firstCreatedDate = DateTime.UtcNow.AddHours(-1);
            var otherCreatedDate = DateTime.UtcNow;

            // Arrange
            await _diagnosisKeysContext.BulkDeleteAsync(_diagnosisKeysContext.DiagnosisKeys.ToList());

            _diagnosisKeysContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false, firstCreatedDate));
            _diagnosisKeysContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false, otherCreatedDate));
            _diagnosisKeysContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false, otherCreatedDate));
            _diagnosisKeysContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xB }, 1, 144, false, otherCreatedDate));
            _diagnosisKeysContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xC }, 1, 144, false, otherCreatedDate));
            await _diagnosisKeysContext.SaveChangesAsync();

            var sut = CreateCommand();

            // Act
            await sut.ExecuteAsync();

            // Assert
            // For Keydata '0xA', expected a single entity having the first Created date and Not published locally
            Assert.Single(_diagnosisKeysContext.DiagnosisKeys.Where(x => x.Created == firstCreatedDate && x.PublishedLocally == false && x.DailyKey.KeyData == new byte[] { 0xA }));
            // For Keydata '0xA', expected 2 entities having a later Created date and both published locally
            Assert.Equal(2, _diagnosisKeysContext.DiagnosisKeys.Count(x => x.Created == otherCreatedDate && x.PublishedLocally == true && x.DailyKey.KeyData == new byte[] { 0xA }));
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
                Created = created ?? DateTime.UtcNow
            };
        }

        private RemoveDuplicateDiagnosisKeysCommand CreateCommand()
        {
            return new RemoveDuplicateDiagnosisKeysCommand(_diagnosisKeysContext);
        }
    }
}
