// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Text.Json;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.GenericContent
{
    public class GenericContentValidator
    {
        public bool IsValid(GenericContentArgs args)
        {
            if (args == null)
                return false;

            if (!GenericContentTypes.IsValid(args.GenericContentType))
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
            catch (JsonException ex)
            {
                //TODO log
                return false;
            }
        }
    }
}