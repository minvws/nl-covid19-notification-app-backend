namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Workflow.DecoyKeys
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Xunit;
    using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;
    using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
    using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Stubs;
    using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.DecoyKeys;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    public class DecoyTimeFilterTest
    {
        [Theory]
        [InlineData(42)]
        [InlineData(343)]
        [InlineData(2416)]
        [InlineData(10000)]
        public async void DecoyTimeAttribute_ExecutionTakesAtleastNMilliseconds(int delayMs)
        {
            //Arrange
            var sut = new DecoyTimeGeneratorAttribute(
                new TestLogger<DecoyTimeGeneratorAttribute>(), 
                new TestRng(delayMs), 
                new DefaultDecoyKeysConfig());
            
            var actionContext = new ActionContext(
                    new DefaultHttpContext(),
                    new RouteData(),
                    new ActionDescriptor(),
                    new ModelStateDictionary());

            var actionExecutingContext = new ActionExecutingContext(
                    actionContext,
                    new List<IFilterMetadata>(),
                    new Dictionary<string, object>(),
                    null);
            
            ActionExecutionDelegate nullAction = async () => { return null; };
            
            var timer = new Stopwatch();

            //Act
            timer.Start();
            await sut.OnActionExecutionAsync(actionExecutingContext, nullAction);
            timer.Stop();
            
            //Assert
            Assert.True(timer.ElapsedMilliseconds >= delayMs, $"Recorded time: {timer.ElapsedMilliseconds}ms.");
        }

        private class TestRng : IRandomNumberGenerator
        {
            private readonly int _Result;
            
            public TestRng(int result) => _Result = result;
            
            public int Next(int min, int max) => _Result;
            
            //ncrunch: no coverage start
            public byte[] NextByteArray(int _)
            {
                throw new NotImplementedException();
            }
            //ncrunch: no coverage end
        }

        private class TestDecoyKeysConfig
        {
            public int MinimumDelayInMilliseconds => 0;
            public int MaximumDelayInMilliseconds => 10000;
        }
    }
}
