// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Mvc;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle
{
    public class HttpPostRivmAdviceCommand
    {
        private readonly RivmAdviceInsertDbCommand _Writer;
        private readonly RivmAdviceValidator _Validator;

        public HttpPostRivmAdviceCommand(RivmAdviceInsertDbCommand writer, RivmAdviceValidator validator)
        {
            _Writer = writer;
            _Validator = validator;
        }

        public IActionResult Execute(MobileDeviceRivmAdviceArgs args)
        {
            if (!_Validator.Valid(args))
                return new BadRequestResult();

            _Writer.Execute(args);
            return new OkResult();
        }
    }
}