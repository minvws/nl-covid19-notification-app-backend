// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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
            context.SaveChanges();

            var sut = CreateCommand();

            // Act
            await sut.ExecuteAsync();

            // Assert
            using var resultContext = _DkSourceDbProvider.CreateNew();
            Assert.Equal(2, resultContext.DiagnosisKeys.Count(_ => _.PublishedToEfgs));
            Assert.Equal(resultContext.DiagnosisKeys.Count(), resultContext.DiagnosisKeys.Count(_ => _.PublishedToEfgs));
        }

        [Fact]
        public async void Tests_that_when_DK_has_been_published_that_all_duplicates_marked_as_published()
        {
            // Assemble
            using var context = _DkSourceDbProvider.CreateNew();
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, true));
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false));
            context.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false));
            context.SaveChanges();

            var sut = CreateCommand();

            // Act
            await sut.ExecuteAsync();

            // Assert
            using var resultContext = _DkSourceDbProvider.CreateNew();
            Assert.Equal(3, resultContext.DiagnosisKeys.Count(_ => _.PublishedToEfgs));
            Assert.Equal(resultContext.DiagnosisKeys.Count(), resultContext.DiagnosisKeys.Count(_ => _.PublishedToEfgs));
        }

        [Fact]
        public async void Tests_that_when_DK_has_not_been_published_that_all_duplicates_except_the_highest_TRL_are_marked_as_published()
        {
            // Assemble
            
            using var assembleContext = _DkSourceDbProvider.CreateNew();
            assembleContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false, TransmissionRiskLevel.Low));
            assembleContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false, TransmissionRiskLevel.Low));
            assembleContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, false, TransmissionRiskLevel.High));
            assembleContext.SaveChanges();

            var sut = CreateCommand();

            // Act
            await sut.ExecuteAsync();

            // Assert
            using var resultContext = _DkSourceDbProvider.CreateNew();
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


        private IRemoveDuplicateDiagnosisKeysForIksCommand CreateCommand()
        {
            return new RemoveDuplicateDiagnosisKeysForIksWithSpCommand(() => _DkSourceDbProvider.CreateNew());
        }

        private void InitSp()
        {
            var sp = @"
CREATE PROCEDURE RemoveDuplicateDiagnosisKeysForIks
AS
BEGIN

	DECLARE @keyData varbinary(max);
	DECLARE @rsn int;
	DECLARE @rp int;
	
	-- Get the natural IDs of all duplicate keys (this is one row per key)
	DECLARE duplicates_cursor CURSOR FOR
	SELECT [DailyKey_KeyData], [DailyKey_RollingStartNumber], [DailyKey_RollingPeriod]
	FROM [dbo].[DiagnosisKeys]
	GROUP BY [DailyKey_KeyData], [DailyKey_RollingStartNumber], [DailyKey_RollingPeriod]
	HAVING count(*) > 1;

	OPEN duplicates_cursor

	FETCH NEXT FROM duplicates_cursor INTO @keyData, @rsn, @rp

	WHILE @@FETCH_STATUS = 0
	BEGIN
		--
		-- Mark ALL as Published if any has been published
		IF EXISTS(
			SELECT 1
			FROM [dbo].[DiagnosisKeys]
			WHERE [DailyKey_KeyData] = @keyData AND [DailyKey_RollingStartNumber] = @rsn AND [DailyKey_RollingPeriod] = @rp
			  AND [PublishedToEfgs] = 0x1
		)
		BEGIN
			UPDATE [dbo].[DiagnosisKeys]
			SET [PublishedToEfgs] = 0x1
			WHERE [DailyKey_KeyData] = @keyData AND [DailyKey_RollingStartNumber] = @rsn AND [DailyKey_RollingPeriod] = @rp 
		END

		--
		-- Mark all except the row with the highest TRL if non are published
		IF EXISTS(
			SELECT 1
			FROM [dbo].[DiagnosisKeys]
			WHERE [DailyKey_KeyData] = @keyData AND [DailyKey_RollingStartNumber] = @rsn AND [DailyKey_RollingPeriod] = @rp
			  AND [PublishedToEfgs] = 0x0
		)
		BEGIN
			UPDATE [dbo].[DiagnosisKeys]
			SET [PublishedToEfgs] = 0x1
			WHERE [DailyKey_KeyData] = @keyData AND [DailyKey_RollingStartNumber] = @rsn AND [DailyKey_RollingPeriod] = @rp
              AND Id NOT IN (
				SELECT TOP 1 Id
				FROM [dbo].[DiagnosisKeys]
				WHERE [DailyKey_KeyData] = @keyData AND [DailyKey_RollingStartNumber] = @rsn AND [DailyKey_RollingPeriod] = @rp
					AND [Local_TransmissionRiskLevel] = (
						SELECT MAX([Local_TransmissionRiskLevel])
						FROM [dbo].[DiagnosisKeys]
						WHERE [DailyKey_KeyData] = @keyData AND [DailyKey_RollingStartNumber] = @rsn AND [DailyKey_RollingPeriod] = @rp
				)
			)
		END

		FETCH NEXT FROM duplicates_cursor INTO @keyData, @rsn, @rp
	END

	CLOSE duplicates_cursor
	DEALLOCATE duplicates_cursor

END";
            using var ctx = _DkSourceDbProvider.CreateNew();
            ctx.Database.ExecuteSqlRaw(sp);
        }
    }
}
