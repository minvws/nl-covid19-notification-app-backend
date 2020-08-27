using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
using System;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngineTests
{
    public class EksStuffingGeneratorTests
    {
        [Fact]
        public void EksStuffingGeneratorTest()
        {
            var tvc = new Mock<ITekValidatorConfig>().Object;

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new EksStuffingGenerator(new StandardRandomNumberGenerator(), tvc).Execute(new StuffingArgs {Count = 0});
            });
        } //crunch: no coverage

    }
}