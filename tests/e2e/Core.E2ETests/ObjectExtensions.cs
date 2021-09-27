// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Text;
using System.Text.Json;

namespace Core.E2ETests
{
    public static class ObjectExtensions
    {
        public static byte[] ToByteArray(this object obj)
        {
            if (obj == null)
            {
                return null;
            }

            var jsonObject = ToJson(obj);

            return Encoding.UTF8.GetBytes(jsonObject);
        }

        public static string ToJson(this object obj)
        {
            if (obj == null)
            {
                return null;
            }

            return JsonSerializer.Serialize(obj, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}
