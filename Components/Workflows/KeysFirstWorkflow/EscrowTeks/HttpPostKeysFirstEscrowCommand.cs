// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.EscrowTeks
{
    public class HttpPostKeysFirstEscrowCommand
    {
        private readonly IKeysFirstEscrowWriter _KeysFirstEscrowDbInsertCommand;
        private readonly IKeysFirstEscrowValidator _KeysFirstEscrowValidator;
        private readonly IDbContextProvider<WorkflowDbContext> _DbContextProvider;

        public HttpPostKeysFirstEscrowCommand(IKeysFirstEscrowWriter infectionDbInsertCommand, IKeysFirstEscrowValidator keysFirstEscrowValidator, IDbContextProvider<WorkflowDbContext> dbContextProvider)
        {
            _KeysFirstEscrowDbInsertCommand = infectionDbInsertCommand;
            _KeysFirstEscrowValidator = keysFirstEscrowValidator;
            _DbContextProvider = dbContextProvider;
        }

        public IActionResult Execute(KeysFirstEscrowArgs args)
        {
            if (!_KeysFirstEscrowValidator.Validate(args))
                return new BadRequestResult();

            using (var tx = _DbContextProvider.BeginTransaction())
            {
                _KeysFirstEscrowDbInsertCommand.Execute(args).GetAwaiter().GetResult();
                _DbContextProvider.Current.SaveChanges();
                tx.Commit();
            }

            return new OkResult();
        }
    }
}