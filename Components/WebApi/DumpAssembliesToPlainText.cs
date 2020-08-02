// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi
{
    public class DumpAssembliesToPlainText
    {
        public IActionResult Execute()
        {
            var list = new List<string>();
            list.AddRange(Assembly.GetEntryAssembly().Dump());
            list.Add(Environment.NewLine);
            list.AddRange(typeof(AssemblyStuff).Assembly.Dump());
            var plainText = string.Join(Environment.NewLine, list);
            return new ContentResult
            {
                ContentType = MediaTypeNames.Text.Plain,
                StatusCode = 200,
                Content = plainText
            };
        }
    }
}
