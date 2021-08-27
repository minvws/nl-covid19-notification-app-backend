// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Core.E2ETests;
using Endpoint.Tests.ContentModels;
using Google.Protobuf;
using Iks.Protobuf;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Protobuf;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.GeneratedGaenFormat;

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

        protected async Task<List<string>> ParseEksContent(Stream stream)
        {
            using var archive = new ZipArchive(stream);

            var content = archive.ReadEntry(ZippedContentEntryNames.EksContent);

            var parser = new MessageParser<TemporaryExposureKeyExport>(() => new TemporaryExposureKeyExport());
            var result = parser.ParseFrom(content);

            var keys = new List<string>();

            foreach (var temporaryExposureKey in result.Keys)
            {
                keys.Add(temporaryExposureKey.KeyData.ToBase64());
            }

            return keys;
        }

        public async Task RemoveExcessBytes(ZipArchive eksZip)
        {
            var gaenSigEntry = eksZip.GetEntry(ZippedContentEntryNames.EksGaenSig);
            using var entryStream = gaenSigEntry.Open();

            var signatureData = TEKSignatureList.Parser.ParseFrom(entryStream);

            await eksZip.ReplaceEntryAsync(ZippedContentEntryNames.EksGaenSig,
                signatureData.Signatures[0].Signature.ToByteArray());
        }

        public async Task<(HttpResponseMessage, T)> GetCdnContent<T>(Uri uri, string version, string endpoint)
        {
            _client = new HttpClient { BaseAddress = uri };
            var responseMessage = await _client.GetAsync($"{version}/{endpoint}");

            return (responseMessage, ParseContent<T>(await responseMessage.Content.ReadAsStreamAsync()));
        }

        public async Task<(HttpResponseMessage, List<string>)> GetCdnEksContent<T>(Uri uri, string version, string endpoint)
        {
            _client = new HttpClient { BaseAddress = uri };
            var responseMessage = await _client.GetAsync($"{version}/{endpoint}");

            return (responseMessage, await ParseEksContent(await responseMessage.Content.ReadAsStreamAsync()));
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
