// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Generic;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.Interfaces;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core
{
    public class CommandInvoker
    {
        private readonly List<ICommand> _commands;

        public CommandInvoker()
        {
            _commands = new List<ICommand>();
        }

        public void SetCommand(ICommand command)
        {
            if (!_commands.Contains(command))
            {
                _commands.Add(command);
            }
        }
        public async Task RunAsync()
        {
            foreach (var command in _commands)
            {
                _ = await command.ExecuteAsync();
            }
        }
    }
}
