// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle
{
    public class HttpPostResourceBundleCommand
    {
        private readonly ResourceBundleInsertDbCommand _Writer;
        private readonly ResourceBundleValidator _Validator;

        public HttpPostResourceBundleCommand(ResourceBundleInsertDbCommand writer, ResourceBundleValidator validator)
        {
            _Writer = writer;
            _Validator = validator;
        }

        public async Task<IActionResult> Execute(ResourceBundleArgs args)
        {
            if (!_Validator.Valid(args))
                return new BadRequestResult();

            await _Writer.Execute(args);
            return new OkResult();
        }
    }
}