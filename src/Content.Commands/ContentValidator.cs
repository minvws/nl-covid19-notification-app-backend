// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    public class ContentValidator
    {
        public bool IsValid(ContentArgs args)
        {
            if (args == null)
                return false;

            if (!ContentTypes.IsValid(args.ContentType))
                return false;

            if (!IsValidJson(args.Json))
                return false;

            return true;
        }

        private bool IsValidJson(string json)
        {
            try
            {
                using var result = JsonDocument.Parse(json);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}