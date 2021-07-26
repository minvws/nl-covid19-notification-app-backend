// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.Interfaces;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core
{
    /// <summary>
    /// The CommandInvoker can execute a set of commands
    /// </summary>
    public class CommandInvoker
    {
        private readonly List<CommandExecuter> _commands;
        private CommandExecuter _commandExecuter;

        public CommandInvoker()
        {
            _commands = new List<CommandExecuter>();
        }
        /// <summary>
        /// Set a command to be executed
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        public CommandInvoker SetCommand(ICommand<IParameters> command)
        {
            _commandExecuter = new CommandExecuter
            {
                Command = command
            };

            _commands.Add(_commandExecuter);

            return this;
        }

        /// <summary>
        /// Set a command to be executed
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        public CommandInvoker SetCommand(ICommand<IParameters> command, IParameters parameters)
        {
            _commandExecuter = new CommandExecuter
            {
                Command = command,
                Parameters = parameters
            };

            _commands.Add(_commandExecuter);

            return this;
        }

        /// <summary>
        /// Adds an Action te be executed before the execution of the command
        /// </summary>
        /// <param name="action">The action to be invoked</param>
        /// <returns></returns>
        public CommandInvoker WithPreExecuteAction(Action action)
        {
            _commandExecuter.BeforeAction = action;
            return this;
        }

        /// <summary>
        /// Adds an Action te be executed after the execution of the command
        /// </summary>
        /// <param name="action">The action to be invoked</param>
        /// <returns></returns>
        public CommandInvoker WithPostExecuteAction(Action action)
        {
            _commandExecuter.AfterAction = action;
            return this;
        }
        
        /// <summary>
        /// Execute all commands in the set of commands
        /// </summary>
        /// <returns></returns>
        public async Task RunAsync()
        {
            foreach (var commandExecuter in _commands)
            {
                await commandExecuter.ExecuteAsync();
            }
        }

        /// <summary>
        /// Execute all commands in the set of commands
        /// All commands will run async internally
        /// </summary>
        public void Run()
        {
            RunAsync().GetAwaiter().GetResult();
        }

        private class CommandExecuter
        {
            public ICommand<IParameters> Command { get; set; }
            public ICommand<IParameters> DependsOnCommand { get; set; }
            public Action BeforeAction { get; set; }
            public Action AfterAction { get; set; }
            public dynamic Parameters { get; set; }

            public async Task ExecuteAsync()
            {
                BeforeAction?.Invoke();
                await Command.ExecuteAsync(Parameters);
                AfterAction?.Invoke();
            }
        }
    }
}
