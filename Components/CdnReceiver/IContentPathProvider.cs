// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace CdnDataReceiver.Controllers
{
    public interface IContentPathProvider
    {
        string Manifest { get; }
        string AppConfig { get; }
        string ResourceBundle { get; }
        string RiskCalculationParameters { get; }
        string ExposureKeySet { get; }
    }
}