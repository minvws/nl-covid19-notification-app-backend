using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngineTests
{
    [TestClass()]
    public class EksStuffingGeneratorTests
    {
        [TestMethod()]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void EksStuffingGeneratorTest()
        {
            var tvc = new Mock<ITekValidatorConfig>().Object;
            new EksStuffingGenerator(new StandardRandomNumberGenerator(), tvc).Execute(new StuffingArgs {Count = 0});
        } //crunch: no coverage

    }
}