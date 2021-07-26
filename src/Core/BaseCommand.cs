// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.Interfaces;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core
{
    /// <summary>
    /// Abstract Command base class
    /// </summary>
    public abstract class BaseCommand : ICommand<IParameters>
    {
        public virtual async Task<ICommandResult> ExecuteAsync(IParameters parameters)
        {
            if (parameters == null)
            {
                return await ExecuteAsync();
            }

            return new CommandResult();
        }

        public virtual async Task<ICommandResult> ExecuteAsync() => await Task.Run(() => new CommandResult());
        public class Parameters : IParameters
        { }
    }
}
