// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    //public class CdnContentHttpReader
    //{
    //    public async Task<BinaryContentResponse?> Execute(string uri2)
    //    {
    //        if (!Uri.TryCreate(uri2, UriKind.Absolute, out var uri))
    //            throw new ArgumentException();

    //        return await BinaryContentResponse(uri);
    //    }

    //    public async Task<BinaryContentResponse?> Execute(string baseUri, string id)
    //    {
    //        //TODO validate id as a publishingId

    //        if (!Uri.TryCreate($"{baseUri}/{id}", UriKind.Absolute, out var uri))
    //            throw new ArgumentException();

    //        return await BinaryContentResponse(uri);
    //    }

    //    private static async Task<BinaryContentResponse?> BinaryContentResponse(Uri uri)
    //    {
    //        using var client = new HttpClient();
    //        client.BaseAddress = uri;
    //        client.DefaultRequestHeaders.Accept.Clear();
    //        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("x-protobuf"));
    //        var response = await client.GetAsync(client.BaseAddress);

    //        if (!response.IsSuccessStatusCode)
    //        {
    //            if (response.StatusCode == HttpStatusCode.NotFound)
    //                return null;

    //            throw new InvalidOperationException("Unexpected response to data call.");
    //        }

    //        var bytes = await response.Content.ReadAsByteArrayAsync();
    //        using var stream = new MemoryStream(bytes);
    //        return ProtoBuf.Serializer.Deserialize<BinaryContentResponse>(stream);
    //    }
    //}

    //public class HttpGetContent<T> where T : ContentEntity
    //{
    //    private IDbContextProvider<ExposureContentDbContext> db;

    //    public IActionResult Arse(string id)
    //    {
    //        if (string.IsNullOrWhiteSpace(id))
    //            return new BadRequestResult();

    //        if (Convert.TryFromBase64String(id, new Span<byte>(), out var length) && length == 256) //TODO config
    //            return new BadRequestResult();

    //        //TODO anything else to mitigate DDOS?

    //        var e = db.Current.Set<T>()
    //            .SingleOrDefault(x => x.PublishingId == id);

    //        if (e == null)
    //            return new NotFoundResult();

    //        var r = new BinaryContentResponse
    //        {
    //            LastModified = e.Release,
    //            PublishingId = e.PublishingId,
    //            ContentTypeName = e.ContentTypeName,
    //            Content = e.Content
    //        };

    //        return new OkObjectResult(r);
    //    }
    //}
}