// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

//using System;
//using System.Text;
//using Microsoft.AspNetCore.Mvc;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

//namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle
//{

//    [Obsolete("Use BinaryContentResponse mechanism.")]
//    public class HttpGetResourceBundleCommand
//    {
//        private readonly SafeGetResourceBundleCommand _Reader;
//        private readonly IPublishingId _PublishingId;

//        public HttpGetResourceBundleCommand(SafeGetResourceBundleCommand reader, IPublishingId publishingId)
//        {
//            _Reader = reader;
//            _PublishingId = publishingId;
//        }

//        public IActionResult Execute(string id)
//        {
//            if (string.IsNullOrWhiteSpace(id))
//                return new BadRequestResult();

//            var parsed = _PublishingId.ParseUri(id);
//            var e = _Reader.Execute(parsed);

//            if (e == null)
//                return new NotFoundResult();

//            // TODO use IJsonSerializer
//            var content = JsonConvert.DeserializeObject<ResourceBundleEntityContent>(Encoding.UTF8.GetString(e.Content));

//            var result = new ResourceBundleResponse
//            {
//                Id = id,
//                IsolationPeriodDays = content.IsolationPeriodDays,
//                TemporaryExposureKeyRetentionDays = content.TemporaryExposureKeyRetentionDays,
//                ObservedTemporaryExposureKeyRetentionDays = content.ObservedTemporaryExposureKeyRetentionDays,
//                Text = content.Text
//            };

//            return new OkObjectResult(result);
//        }
//    }
//}