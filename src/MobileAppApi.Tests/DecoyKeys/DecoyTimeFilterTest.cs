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
        public void DecoyTimeAttribute_ExecutionTakesAtleastNMilliseconds(int delayMs)
        {
            //Arrange
            var mockTimeCalculator = new Mock<IDecoyTimeCalculator>();
            mockTimeCalculator.Setup(x => x.GetDelay())
                .Returns(TimeSpan.FromMilliseconds(delayMs));

            var sut = new DecoyTimeGeneratorAttribute(mockTimeCalculator.Object);

            var actionContext = new ActionContext(
                new DefaultHttpContext(),
                new RouteData(),
                new ActionDescriptor(),
                new ModelStateDictionary());

            var resultExecutingContext = new ResourceExecutedContext(
                actionContext,
                new List<IFilterMetadata>());

            var timer = new Stopwatch();

            //Act
            timer.Start();
            sut.OnResourceExecuted(resultExecutingContext);
            timer.Stop();

            //Assert
            // 10% margin, as Task.Delay has some inaccuracies
            Assert.True(timer.ElapsedMilliseconds >= 0.90 * delayMs, $"Recorded time: {timer.ElapsedMilliseconds}ms.");
        }
    }
}
