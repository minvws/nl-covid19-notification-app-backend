// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core
{
    /// <summary>
    /// The CommandInvoker can execute a set of commands
    /// </summary>
    public class CommandInvoker
    {
        private readonly List<CommandExecutor> _commands;
        private CommandExecutor _commandExecutor;

        public CommandInvoker()
        {
            _commands = new List<CommandExecutor>();
        }

        /// <summary>
        /// Set a command to be executed
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        public CommandInvoker SetCommand(ICommand<IParameters> command)
        {
            _commandExecutor = new CommandExecutor
            {
                Command = command
            };

            _commands.Add(_commandExecutor);

            return this;
        }

        /// <summary>
        /// Set a command to be executed
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        public CommandInvoker SetCommand(ICommand<IParameters> command, IParameters parameters)
        {
            _commandExecutor = new CommandExecutor
            {
                Command = command,
                Parameters = parameters
            };

            _commands.Add(_commandExecutor);

            return this;
        }

        /// <summary>
        /// Adds an Action to be executed before the execution of the command
        /// </summary>
        /// <param name="action">The action to be invoked</param>
        /// <returns></returns>
        public CommandInvoker WithPreExecuteAction(Action action)
        {
            _commandExecutor.BeforeAction = action;
            return this;
        }

        /// <summary>
        /// Adds an Action to be executed after the execution of the command
        /// </summary>
        /// <param name="action">The action to be invoked</param>
        /// <returns></returns>
        public CommandInvoker WithPostExecuteAction(Action action)
        {
            _commandExecutor.AfterAction = action;
            return this;
        }

        /// <summary>
        /// Execute all commands in the set of commands
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteAsync()
        {
            foreach (var commandExecutor in _commands)
            {
                await commandExecutor.ExecuteAsync();
            }
        }

        /// <summary>
        /// Execute all commands in the set of commands
        /// All commands will run async internally
        /// </summary>
        public void Execute()
        {
            ExecuteAsync().GetAwaiter().GetResult();
        }

        private class CommandExecutor
        {
            public ICommand<IParameters> Command { get; set; }
            public Action BeforeAction { get; set; }
            public Action AfterAction { get; set; }
            public IParameters Parameters { get; set; }

            public async Task ExecuteAsync()
            {
                BeforeAction?.Invoke();
                await Command.ExecuteAsync(Parameters);
                AfterAction?.Invoke();
            }
        }
    }
}
