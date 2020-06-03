// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Http;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets
{
    /// <summary>
    /// TODO depending on accept-headers grab the right content and return
    /// </summary>
    public class HttpGetAgExposureKeySetCommand
    {
        private readonly AgExposureKeySetSafeReadCommand _SafeReadcommand;

        public HttpGetAgExposureKeySetCommand(AgExposureKeySetSafeReadCommand safeReadcommand)
        {
            _SafeReadcommand = safeReadcommand;
        }

        public void Execute(string id, ExposureKeySetFormat resultFormat, HttpRequest httpRequest)
        {
            //var ExposureKeySet = _SafeReadcommand.Execute(id);

            //if (ExposureKeySet == null)
            //    return new NotFoundResult();

            //if (resultFormat == ExposureKeySetFormat.Ag)
            //    return new FileContentResult(ExposureKeySet.AgContent, MediaTypeHeaderValue.Parse("application/zip"));


            //return new ContentResult {Content = ExposureKeySet.JsonContent, StatusCode = 200, ContentType = "application/json"};
        }
    }
}