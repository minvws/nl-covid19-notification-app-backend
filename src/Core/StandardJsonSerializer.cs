// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Text.Json;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core
{
    public class StandardJsonSerializer : IJsonSerializer
    {
        private readonly JsonSerializerOptions _serializerOptions;

        public StandardJsonSerializer()
        {
            _serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        }

        public string Serialize<TContent>(TContent input)
        {
            return JsonSerializer.Serialize(input, _serializerOptions);
        }

        public TContent Deserialize<TContent>(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException(nameof(input)); //Standardising.
            }

            return JsonSerializer.Deserialize<TContent>(input, _serializerOptions);
        }
    }
}
