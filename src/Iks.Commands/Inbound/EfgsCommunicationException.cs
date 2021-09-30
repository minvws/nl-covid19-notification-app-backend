// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Inbound
{
    public class EfgsCommunicationException : Exception
    {
        public EfgsCommunicationException()
        {
        }

        public EfgsCommunicationException(string efgsMessage) : base(efgsMessage)
        {
        }
    }
}
