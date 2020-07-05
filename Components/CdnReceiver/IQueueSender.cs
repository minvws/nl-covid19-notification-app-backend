// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver
{
    public interface IQueueSender<T>
    {
        Task Send(T message);
    }

    public class NotAQueueSender<T> : IQueueSender<T>
    {
        public Task Send(T message)
        {
            //Not a sausage.
            return Task.CompletedTask;
        }
    }
}