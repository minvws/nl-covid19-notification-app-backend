// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest
{
    public interface IAgExposureKeySetConfigEntityWriter
    {
        Task<ExposureKeySetContentEntity> Execute(ExposureKeySetContentEntity args);
    }
}