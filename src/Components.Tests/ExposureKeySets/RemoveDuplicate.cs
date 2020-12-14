// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using System;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.ExposureKeySets
{
    public class RemoveDuplicateDiagnosisKeysCommandTests
    {
        [Fact]
        public async void Tests_that_no_action_taken_for_published_duplicates()
        {
            // Assemble
            Func<DkSourceDbContext> factory = () => new DkSourceDbContext(new DbContextOptions<DkSourceDbContext>());
            var sut = new RemoveDuplicateDiagnosisKeysForIksCommand(factory);

            // Act
            await sut.ExecuteAsync();

            // Assert
        }

        [Fact]
        public async void Tests_that_when_DK_has_been_published_that_all_duplicates_marked_as_published()
        {
            // Assemble
            Func<DkSourceDbContext> factory = () => new DkSourceDbContext(new DbContextOptions<DkSourceDbContext>());
            var sut = new RemoveDuplicateDiagnosisKeysForIksCommand(factory);

            // Act
            await sut.ExecuteAsync();

            // Assert
        }

        [Fact]
        public async void Tests_that_when_DK_has_not_been_published_that__all_duplicates_except_the_highest_TRL_are_marked_as_published()
        {
            // Assemble
            Func<DkSourceDbContext> factory = () => new DkSourceDbContext(new DbContextOptions<DkSourceDbContext>());
            var sut = new RemoveDuplicateDiagnosisKeysForIksCommand(factory);

            // Act
            await sut.ExecuteAsync();

            // Assert
        }
    }
}
