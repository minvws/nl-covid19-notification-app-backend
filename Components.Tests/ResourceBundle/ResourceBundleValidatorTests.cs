// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.ResourceBundle
{
    [TestClass()]
    public class ResourceBundleValidatorTests
    {
        [TestMethod()]
        public void IsBase64Test()
        {
            var thing = Convert.ToBase64String(Encoding.UTF8.GetBytes("Klingons off the starboard bow!"));
            Assert.IsTrue(ResourceBundleValidator.IsBase64(thing));
        }

        [TestMethod()]
        public void DoesCultureExistTest()
        {
            Assert.IsTrue(ResourceBundleValidator.CultureExists("en"));
            Assert.IsTrue(ResourceBundleValidator.CultureExists("en-US"));
            Assert.IsTrue(ResourceBundleValidator.CultureExists("nl"));
            Assert.IsTrue(ResourceBundleValidator.CultureExists("nl-NL"));
            Assert.IsTrue(ResourceBundleValidator.CultureExists("nl-BE"));
            Assert.IsTrue(ResourceBundleValidator.CultureExists("fr-BE"));
        }

        [TestMethod()]
        public void WhatDoesDicOfDicLookLike()
        {
            IJsonSerializer jsonSerializer = new StandardJsonSerializer();

            var argle = new ResourceBundleArgs
            {
                Text = new Dictionary<string, Dictionary<string, string>>
                {
                    {
                        "en-GB", new Dictionary<string, string>()
                        {
                            {"InfectedMessage", "You're possibly infected"}
                        }
                    },
                    {
                        "nl-NL", new Dictionary<string, string>
                        {
                            {"InfectedMessage", "U bent mogelijk geïnfecteerd"}
                        }
                    }
                }
            };

            var stuff = jsonSerializer.Serialize(argle);
        }
    }
}