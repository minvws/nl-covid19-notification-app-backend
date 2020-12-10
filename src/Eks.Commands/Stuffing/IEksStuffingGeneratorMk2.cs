// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.Stuffing
{
    public interface IEksStuffingGeneratorMk2
    {
        EksCreateJobInputEntity[] Execute(int count);
    }
}