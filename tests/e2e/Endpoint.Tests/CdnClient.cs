// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Endpoint.Tests.ContentModels;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace Endpoint.Tests
{
    public class CdnClient
    {
        private readonly IJsonSerializer _jsonSerializer;
        private HttpClient _client;

        public CdnClient()
        {
            _jsonSerializer = new StandardJsonSerializer();
        }

        protected T ParseContent<T>(Stream stream)
        {
            using var archive = new ZipArchive(stream);

            var content = archive.ReadEntry(ZippedContentEntryNames.Content);
            return _jsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(content));
        }

        protected T ParseEksContent<T>(Stream stream)
        {
            using var archive = new ZipArchive(stream);

            var content = archive.ReadEntry(ZippedContentEntryNames.EksContent);
            var protoBufParser = new ProtoBufParser();
            return protoBufParser.Parse<T>(content);
        }

        public async Task<(HttpResponseMessage, T)> GetCdnContent<T>(Uri uri, string version, string endpoint)
        {
            _client = new HttpClient { BaseAddress = uri };
            var responseMessage = await _client.GetAsync($"{version}/{endpoint}");

            if(endpoint.StartsWith("exposurekeyset"))
            {
                return (responseMessage, ParseEksContent<T>(await responseMessage.Content.ReadAsStreamAsync()));
            }

            return (responseMessage, ParseContent<T>(await responseMessage.Content.ReadAsStreamAsync()));
        }
    }

    public class ProtoBufParser
    {
        public T Parse<T>(byte[] content)
        {
            var eks = (T)Activator.CreateInstance(typeof(ExposureKeySet));
            if (eks is ExposureKeySet set)
            {
                set.eksData = content;
            }

            return eks;
        }
    }
}
