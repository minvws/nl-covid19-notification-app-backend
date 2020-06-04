// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps.Arguments;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps.Commands
{
    public class HttpPostGenerateWorkflowCommand
    {
        private readonly IDbContextProvider<ExposureContentDbContext> _contextProvider;

        public HttpPostGenerateWorkflowCommand(IDbContextProvider<ExposureContentDbContext>contextProvider)
        {
            _contextProvider = contextProvider;
        }

        public IActionResult Execute(HttpPostGenerateWorkflowArguments args)
        {
            try
            {
                if (args.WorkflowCount < 1)
                    throw new ArgumentOutOfRangeException(nameof(args.WorkflowCount));
                
                var r = new Random(args.RandomSeed); //Don't need the crypto version.
                var c1 = new GenerateWorkflows(_contextProvider);
                c1.Execute(args.WorkflowCount, x => r.Next(x), x => r.NextBytes(x));

                return new OkResult();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return new BadRequestResult();
            }
        }
    }
}
