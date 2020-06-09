// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.EscrowTeks
{
    public class HttpPostKeysFirstEscrowCommand
    {
        private readonly IKeysFirstEscrowWriter _KeysFirstEscrowDbInsertCommand;
        private readonly IKeysFirstEscrowValidator _KeysFirstEscrowValidator;
        private readonly WorkflowDbContext _DbContextProvider;

        public HttpPostKeysFirstEscrowCommand(IKeysFirstEscrowWriter infectionDbInsertCommand, IKeysFirstEscrowValidator keysFirstEscrowValidator, WorkflowDbContext dbContextProvider)
        {
            _KeysFirstEscrowDbInsertCommand = infectionDbInsertCommand;
            _KeysFirstEscrowValidator = keysFirstEscrowValidator;
            _DbContextProvider = dbContextProvider;
        }

        public async Task<IActionResult> Execute(KeysFirstEscrowArgs args)
        {
            if (!_KeysFirstEscrowValidator.Validate(args))
                return new BadRequestResult();

            await using (var tx = _DbContextProvider.BeginTransaction())
            {
                await _KeysFirstEscrowDbInsertCommand.Execute(args);
                _DbContextProvider.SaveAndCommit();
            }

            return new OkResult();
        }
    }
}