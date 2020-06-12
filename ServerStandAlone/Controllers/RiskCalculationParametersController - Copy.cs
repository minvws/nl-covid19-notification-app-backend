// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ServerStandAlone.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AppConfigController : ControllerBase
    {

        /// <summary>
        /// asdasdasdads
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        /// sdasdasd
        [HttpGet]
        [Route(EndPointNames.CdnApi.AppConfig +"/{id}")]
        public async Task GetLatestConfig(string id, [FromServices]HttpGetCdnContentCommand<AppConfigEntity> command)
        {
            await command.Execute(HttpContext, id);
        }

        [HttpPost]
        [Route(EndPointNames.CdnApi.AppConfig)]
        public async Task<IActionResult> Post([FromBody]AppConfigArgs args, [FromServices]HttpPostAppConfigCommand command)
        {
            return await command.Execute(args);
        }
    }

    public class HttpPostAppConfigCommand
    {
        public async Task<IActionResult> Execute(AppConfigArgs args)
        {
            throw new NotImplementedException("Not implemented.");
        }
    }

    public class AppConfigArgs
    {
        /// <summary>
        /// The minimum version of the app.The app has a hardcoded version number that is increased by 1  on each app release. Whenever the app downloads the manifest, it must compare its hardcoded version  number with that of the manifest. If the hardcoded version number is less than the manifest value, the user will be asked to upgrade the app from the app store.See https://github.com/minvws/nl-covid19-notification-app-coordination/blob/master/architecture/Solution%20Architecture.md#lifecycle-management.
        /// TODO: this must be discussed with the Design team.
        /// </summary>
        [Swashbuckle.AspNetCore.Annotations.SwaggerSchema("asdsdf")]
        public long Version { get; set; }

        ///This defines the frequency of retrieving the manifest, in minutes.
        public int ManifestFrequency { get; set; }

        /// <summary>
        /// This defines the probability of sending decoys. This is configurable so we can tune the probability to server load if necessary.
        /// </summary>
        public int DecoyProbability { get; set; }
    }


    public class AppConfigEntity : ContentEntity
    {
    }
}
