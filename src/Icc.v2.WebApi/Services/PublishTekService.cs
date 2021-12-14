// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.TekPublication;

namespace NL.Rijksoverheid.ExposureNotification.Icc.v2.WebApi.Services
{
    /// <summary>
    /// Service processing all incoming PubTEk values
    /// </summary>
    public class PublishTekService : IPublishTekService
    {
        private readonly PublishTekCommand _publishTekCommand;
        private readonly PublishTekArgsValidator _publishTekArgsValidator;
        private readonly ILogger _logger;

        public PublishTekService(ILogger<PublishTekService> logger, PublishTekCommand publishTekCommand, PublishTekArgsValidator publishTekArgsValidator)
        {
            _logger = logger;

            _publishTekCommand = publishTekCommand ?? throw new ArgumentNullException(nameof(publishTekCommand));
            _publishTekArgsValidator = publishTekArgsValidator ?? throw new ArgumentNullException(nameof(publishTekArgsValidator));
        }

        public async Task<PublishTekResponse> ExecuteAsync(PublishTekArgs args, bool isOriginPortal)
        {
            var response = new PublishTekResponse();

            // Validates the given PubTEK input. If there are errors, set response valid value to false.
            var errors = _publishTekArgsValidator.Validate(args);
            if (errors.Any())
            {
                _logger.LogError("{Messages}.", string.Join(Environment.NewLine, errors));
                response.Valid = false;
            }
            else
            {
                // If valid, try to publish the TEK.
                var result = await _publishTekCommand.ExecuteAsync(new PublishTekCommand.Parameters { PublishTekArgs = args, IsOriginPortal = isOriginPortal });
                response.Valid = !result.HasErrors;
            }

            // Return a response with only true or false. No further information should be send to the client.
            return response;
        }
    }
}
