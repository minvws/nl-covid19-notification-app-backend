// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps.KeysFirstWorkflow
{
    public class HttpPostKeysFirstGenerateTekSetsCommand
    {
        private readonly IDbContextProvider<WorkflowDbContext> _ContextProvider;

        public HttpPostKeysFirstGenerateTekSetsCommand(IDbContextProvider<WorkflowDbContext> contextProvider)
        {
            _ContextProvider = contextProvider;
        }

        public IActionResult Execute(GenerateKeysFirstTekSetsArgs args)
        {
                if (args.WorkflowCount < 1)
                    throw new ArgumentOutOfRangeException(nameof(args.WorkflowCount));
                
                var r = new Random(args.RandomSeed); //Don't need the crypto version.
                var c1 = new GenerateKeysFirstWorkflowItems(_ContextProvider);
                c1.Execute(args.WorkflowCount, x => r.Next(x), x => r.NextBytes((byte[])x));

                return new OkResult();
        }
    }
}
