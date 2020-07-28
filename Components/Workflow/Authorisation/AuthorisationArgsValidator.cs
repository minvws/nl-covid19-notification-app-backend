// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Text.RegularExpressions;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation
{
    public class AuthorisationArgsValidator
    {
        public bool Validate(AuthorisationArgs args)
        {
            if (args == null)
                return false;

            if (string.IsNullOrWhiteSpace(args.LabConfirmationId))
                return false;
            if (args.LabConfirmationId.Length != 6)
                return false;
            if (!Regex.IsMatch(args.LabConfirmationId, "^[BCFGJLQRSTUVXYZ23456789]*$"))
                return false;
            
            // TODO check SymptonsOnDate is valid date in !past!

            return true;
        }

    }
}