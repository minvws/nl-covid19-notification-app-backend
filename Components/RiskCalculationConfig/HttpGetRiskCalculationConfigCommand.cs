// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

//using System;
//using System.Text;
//using System.Web;
//using Microsoft.AspNetCore.Mvc;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

//namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig
//{

//    [Obsolete("Use BinaryContentResponse mechanism.")]
//    public class HttpGetRiskCalculationConfigCommand
//    {
//        private readonly SafeGetRiskCalculationConfigDbCommand _SafeReadcommand;
//        private readonly IPublishingId _PublishingId;

//        public HttpGetRiskCalculationConfigCommand(SafeGetRiskCalculationConfigDbCommand safeReadcommand, IPublishingId publishingId)
//        {
//            _SafeReadcommand = safeReadcommand;
//            _PublishingId = publishingId;
//        }

//        public IActionResult Execute(string id)
//        {
//            if (string.IsNullOrWhiteSpace(id))
//                return new BadRequestResult();

//            var parsed = _PublishingId.ParseUri(id);
//            var e = _SafeReadcommand.Execute(parsed);

//            if (e == null)
//                return new NotFoundResult();

//            // TODO use IJsonSerializer
//            var content = JsonConvert.DeserializeObject<RiskCalculationConfigContent>(Encoding.UTF8.GetString(e.Content));
//            var result = content.ToResponse();
//            return new OkObjectResult(result);
//        }
//    }
//}