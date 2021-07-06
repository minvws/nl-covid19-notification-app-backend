// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Tests.ExposureKeySetsEngine
{
    [Trait("db", "ss")]
    public class RemoveDuplicateDiagnosisKeysForIksCommandTests : IDisposable
    {
        private static DbConnection _connection;
        private readonly DkSourceDbContext _dkSourceContext;

        public RemoveDuplicateDiagnosisKeysForIksCommandTests()
        {
            var dkSourceDbProvider = new DbContextOptionsBuilder<DkSourceDbContext>().UseSqlServer(CreateSqlDatabase()).Options;
            var sp = File.ReadAllText(@"Resources\RemoveDuplicateDiagnosisKeysForIks.sql");
            _dkSourceContext = new DkSourceDbContext(dkSourceDbProvider);
            _dkSourceContext.Database.EnsureDeleted(); // Delete database first because the sp cannot be added twice.
            _dkSourceContext.Database.EnsureCreated();
            _dkSourceContext.Database.ExecuteSqlRaw(sp);
        }

        private static DbConnection CreateSqlDatabase()
        {
            var csb = new SqlConnectionStringBuilder($"Data Source=.;Initial Catalog={nameof(RemoveDuplicateDiagnosisKeysForIksCommandTests) + "_D"};Integrated Security=True")
            {
                MultipleActiveResultSets = true
            };

            _connection = new SqlConnection(csb.ConnectionString);
            return _connection;
        }

        public void Dispose() => _connection.Dispose();

        [Fact]
        public async Task Tests_that_no_action_taken_for_published_duplicates()
        {
            // Arrange
            await _dkSourceContext.TruncateAsync<DiagnosisKeyEntity>();

            _dkSourceContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, true));
            _dkSourceContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, true));
            _dkSourceContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xB }, 1, 144, false));
            _dkSourceContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xC }, 1, 144, false));
            await _dkSourceContext.SaveChangesAsync();

            var sut = CreateCommand();

            // Act
            await sut.ExecuteAsync();

            // Assert
            Assert.Equal(2, _dkSourceContext.DiagnosisKeys.Count(_ => _.PublishedToEfgs));
        }

        [Fact]
        public async Task Tests_that_when_DK_has_been_published_that_all_duplicates_marked_as_published()
        {
            // Arrange
            _dkSourceContext.Truncate<DiagnosisKeyEntity>();

            _dkSourceContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, true));
            _dkSourceContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false));
            _dkSourceContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false));
            _dkSourceContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xB }, 1, 144, false));
            _dkSourceContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xC }, 1, 144, false));
            await _dkSourceContext.SaveChangesAsync();

            var sut = CreateCommand();

            // Act
            await sut.ExecuteAsync();

            // Assert
            Assert.Equal(3, _dkSourceContext.DiagnosisKeys.Count(_ => _.PublishedToEfgs));
        }

        [Fact]
                public async Task Tests_that_when_DK_has_not_been_published_that_all_duplicates_except_the_highest_TRL_are_marked_as_published()
        {
            // Arrange
            await _dkSourceContext.TruncateAsync<DiagnosisKeyEntity>();

            _dkSourceContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false));
            _dkSourceContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false));
            _dkSourceContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false, TransmissionRiskLevel.High));
            _dkSourceContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xB }, 1, 144, false));
            _dkSourceContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xC }, 1, 144, false));
            await _dkSourceContext.SaveChangesAsync();

            var sut = CreateCommand();

            // Act
            await sut.ExecuteAsync();

            // Assert
            Assert.Single(_dkSourceContext.DiagnosisKeys.Where(x => x.PublishedToEfgs == false && x.DailyKey.KeyData == new byte[] { 0xA }));
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

        private IRemoveDuplicateDiagnosisKeysCommand CreateCommand()
        {
            return new RemoveDuplicateDiagnosisKeysForIksWithSpCommand(_dkSourceContext);
        }
    }
}
