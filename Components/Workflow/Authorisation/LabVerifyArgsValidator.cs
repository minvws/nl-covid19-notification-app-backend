// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation
{
    public class LabVerifyArgsValidator
    {
        private readonly PollTokenService _PollTokenService;

        public LabVerifyArgsValidator(PollTokenService pollTokenService)
        {
            _PollTokenService = pollTokenService;
        }

        public bool Validate(LabVerifyArgs args)
        {
            if (args == null)
                return false;

            if (string.IsNullOrWhiteSpace(args.PollToken))
                return false;

            return _PollTokenService.Validate(args.PollToken);
        }
    }
}