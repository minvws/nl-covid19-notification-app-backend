// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands;
using Xunit;

namespace EksEngine.Tests.Commands
{
    public abstract class DiagnosiskeyInputEntityDeduplicatorTests
    {
        private readonly DkSourceDbContext _dkSourceContext;
        private readonly DiagnosiskeyInputEntityDeduplicator _sut;

        public DiagnosiskeyInputEntityDeduplicatorTests(DbContextOptions<DkSourceDbContext> dkSourceContextOptions)
        {
            var lf = new LoggerFactory();

            _dkSourceContext = new DkSourceDbContext(dkSourceContextOptions ?? throw new ArgumentNullException(nameof(dkSourceContextOptions)));
            _dkSourceContext.Database.EnsureCreated();

            _dkSourceContext.BulkDelete(_dkSourceContext.DiagnosisKeys.ToList());

            _sut = new DiagnosiskeyInputEntityDeduplicator(
                _dkSourceContext,
                lf.CreateLogger<DiagnosiskeyInputEntityDeduplicator>());
        }

        [Fact]
        public async Task DkTableEmpty_FilterOutExistingDailyKeys_AllEntitiesReturned()
        {
            //Arrange
            var testInputEntities = new List<DiagnosisKeyInputEntity>
            {
                CreateDkie(new byte[]{0xA}, 1, 144),
                CreateDkie(new byte[]{0xB}, 1, 144),
                CreateDkie(new byte[]{0xC}, 1, 144)
            };

            //Act
            var result = await _sut.FilterOutExistingDailyKeys(testInputEntities);

            //Assert
            result.Count.Should().Be(3);
        }

        [Fact]
        public async Task NoInputEntities_FilterOutExistingDailyKeys_NothingReturned()
        {
            //Arrange
            var testInputEntities = new List<DiagnosisKeyInputEntity>();

            //Act
            var result = await _sut.FilterOutExistingDailyKeys(testInputEntities);

            //Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task NoEqualDailyKeys_FilterOutExistingDailyKeys_AllEntitiesReturned()
        {
            //Arrange
            _dkSourceContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, true));
            _dkSourceContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xB }, 1, 144, false));
            _dkSourceContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xC }, 1, 144, false));
            await _dkSourceContext.SaveChangesAsync();

            var testInputEntities = new List<DiagnosisKeyInputEntity>
            {
                CreateDkie(new byte[]{0xD}, 2, 145),
                CreateDkie(new byte[]{0xE}, 1, 144),
                CreateDkie(new byte[]{0xA}, 2, 144),
                CreateDkie(new byte[]{0xB}, 1, 145)
            };

            //Act
            var result = await _sut.FilterOutExistingDailyKeys(testInputEntities);

            //Assert
            result.Count.Should().Be(4);
        }

        [Fact]
        public async Task TwoEqualDailyKeys_FilterOutExistingDailyKeys_MatchingEntitiesNotReturned()
        {
            //Arrange
            _dkSourceContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, true));
            _dkSourceContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xB }, 1, 144, false));
            _dkSourceContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xC }, 1, 144, false));
            await _dkSourceContext.SaveChangesAsync();

            var testDuplicate = CreateDkie(new byte[] { 0xB }, 1, 144);

            var testInputEntities = new List<DiagnosisKeyInputEntity>
            {
                testDuplicate,
                CreateDkie(new byte[] { 0xE }, 2, 145),
                CreateDkie(new byte[] { 0xA }, 2, 144),
                CreateDkie(new byte[] { 0xB }, 1, 145)
            };

            //Act
            var result = await _sut.FilterOutExistingDailyKeys(testInputEntities);

            //Assert
            result.Count.Should().Be(3);
            result.Should().NotContain(testDuplicate);
        }

        [Fact]
        public async Task AllDailyKeysMatch_FilterOutExistingDailyKeys_NothingReturned()
        {
            //Arrange
            _dkSourceContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xA }, 1, 144, true));
            _dkSourceContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xB }, 1, 144, false));
            _dkSourceContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xC }, 1, 144, false));
            await _dkSourceContext.SaveChangesAsync();

            var testInputEntities = new List<DiagnosisKeyInputEntity>
            {
                CreateDkie(new byte[] { 0xA }, 1, 144),
                CreateDkie(new byte[] { 0xB }, 1, 144),
                CreateDkie(new byte[] { 0xC }, 1, 144)
            };

            //Act
            var result = await _sut.FilterOutExistingDailyKeys(testInputEntities);

            //Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task MatchingDkIsMarkedForCleanup_FilterOutExistingDailyKeys_EntityReturned()
        {
            //Arrange
            var dkTobeCleaned = new DiagnosisKeyEntity()
            {
                DailyKey = new DailyKey
                {
                    KeyData = new byte[] { 0xA },
                    RollingStartNumber = 1,
                    RollingPeriod = 144
                },
                PublishedToEfgs = true,
                PublishedLocally = false,
                ReadyForCleanup = true,
                Local = new LocalTekInfo
                {
                    TransmissionRiskLevel = TransmissionRiskLevel.Low
                },
                Created = DateTime.Now
            };

            var dkNotToBeCleaned = new DiagnosisKeyEntity()
            {
                DailyKey = new DailyKey
                {
                    KeyData = new byte[] { 0xB },
                    RollingStartNumber = 1,
                    RollingPeriod = 144
                },
                PublishedToEfgs = true,
                PublishedLocally = false,
                ReadyForCleanup = false,
                Local = new LocalTekInfo
                {
                    TransmissionRiskLevel = TransmissionRiskLevel.Low
                },
                Created = DateTime.Now
            };

            _dkSourceContext.DiagnosisKeys.Add(dkTobeCleaned);
            _dkSourceContext.DiagnosisKeys.Add(dkNotToBeCleaned);
            _dkSourceContext.DiagnosisKeys.Add(CreateDk(new byte[] { 0xC }, 1, 144, false));
            await _dkSourceContext.SaveChangesAsync();

            var matchingInputEntity = CreateDkie(new byte[] { 0xA }, 1, 144);

            var testInputEntities = new List<DiagnosisKeyInputEntity>
            {
                matchingInputEntity,
                CreateDkie(new byte[] { 0xB }, 1, 144)
            };

            //Act
            var result = await _sut.FilterOutExistingDailyKeys(testInputEntities);

            //Assert
            result.Should().ContainSingle(x => x == matchingInputEntity);
        }

        private static DiagnosisKeyInputEntity CreateDkie(
            byte[] keyData,
            int rollingStartNumber,
            int rollingPeriod,
            TransmissionRiskLevel trl = TransmissionRiskLevel.Low)
        {
            return new DiagnosisKeyInputEntity
            {
                DailyKey = new DailyKey
                {
                    KeyData = keyData,
                    RollingStartNumber = rollingStartNumber,
                    RollingPeriod = rollingPeriod
                },
                Local = new LocalTekInfo
                {
                    TransmissionRiskLevel = trl
                }
            };
        }

        private static DiagnosisKeyEntity CreateDk(
            byte[] keyData,
            int rollingStartNumber,
            int rollingPeriod,
            bool publishedLocally,
            DateTime? created = null,
            TransmissionRiskLevel trl = TransmissionRiskLevel.Low)
        {
            return new DiagnosisKeyEntity
            {
                DailyKey = new DailyKey
                {
                    KeyData = keyData,
                    RollingStartNumber = rollingStartNumber,
                    RollingPeriod = rollingPeriod
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
    }
}
