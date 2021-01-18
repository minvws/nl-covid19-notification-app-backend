// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.DecoyKeys;
using System;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Tests.DecoyKeys
{
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
            var mockTimeCalculator = new Mock<IDecoyTimeCalculator>();
            mockTimeCalculator.Setup(x => x.GenerateDelayTime())
                .Returns(TimeSpan.FromMilliseconds(delayMs));

            var sut = new DecoyTimeGeneratorAttribute(mockTimeCalculator.Object);

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
    }
}
