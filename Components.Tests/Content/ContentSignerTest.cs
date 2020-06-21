// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Content
{
    [TestClass]
    public class ContentSignerTest
    {
        static Random rd = new Random();

        [DataRow(500)]
        [DataRow(1000)]
        [DataRow(2000)]
        [DataRow(3000)]
        [DataRow(10000)]
        [DataTestMethod]
        public void Build(int length)
        {
            var signer = new CmsSigner(new ResourceCertificateProvider("FakeRSA.p12"));
            var content = Encoding.UTF8.GetBytes(CreateString(length));
            var signature = signer.GetSignature(content);
            Assert.IsTrue(signature.Length == signer.LengthBytes);
        }

        internal static string CreateString(int stringLength)
        {
            const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@$?_-";
            char[] chars = new char[stringLength];

            for (int i = 0; i < stringLength; i++)
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }

            return new string(chars);
        }
    }
}
