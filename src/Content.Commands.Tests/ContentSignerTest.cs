// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Tests;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Tests
{
    public class ContentSignerTest
    {
        private static readonly Random _Random = new Random();

        [Theory]
        [InlineData(500)]
        [InlineData(1000)]
        [InlineData(2000)]
        [InlineData(3000)]
        [InlineData(10000)]
        public void Build(int length)
        {
            var lf = new LoggerFactory();
            var content = Encoding.UTF8.GetBytes(CreateString(length));
            var sig = TestSignerHelpers.CreateCmsSignerEnhanced(lf).GetSignature(content);
            Assert.True((sig?.Length ?? 0) != 0);
        }

        internal static string CreateString(int stringLength)
        {
            const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@$?_-";
            var chars = new char[stringLength];

            for (var i = 0; i < stringLength; i++)
            {
                chars[i] = allowedChars[_Random.Next(0, allowedChars.Length)];
            }

            return new string(chars);
        }
    }
}
