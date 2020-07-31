using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization.Internal;
using Microsoft.Extensions.WebEncoders.Testing;

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
