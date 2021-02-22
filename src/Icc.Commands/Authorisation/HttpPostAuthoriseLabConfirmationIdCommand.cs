using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation
{
    public class HttpPostAuthoriseLabConfirmationIdCommand
    {
        private readonly AuthorizeLabConfirmationIdCommand _AuthorisationWriter;
        private readonly AuthorisationArgsValidator _AuthorisationArgsValidator;
        private readonly ILogger _Logger;

        public HttpPostAuthoriseLabConfirmationIdCommand(AuthorizeLabConfirmationIdCommand authorisationWriter, AuthorisationArgsValidator authorisationArgsValidator, ILogger<HttpPostAuthoriseLabConfirmationIdCommand> logger)
        {
            _AuthorisationWriter = authorisationWriter ?? throw new ArgumentNullException(nameof(authorisationWriter));
            _AuthorisationArgsValidator = authorisationArgsValidator ?? throw new ArgumentNullException(nameof(authorisationArgsValidator));
            _Logger = logger;
        }

        public async Task<AuthorisationResponse> ExecuteAsync(AuthorisationArgs args)
        {
            if (_Logger.LogValidationMessages(_AuthorisationArgsValidator.Validate(args)))
                return null;

            var success = await _AuthorisationWriter.ExecuteAsync(args);

            var response = new AuthorisationResponse
            {
                Valid = success,
            };

            return response;
        }
    }
}
