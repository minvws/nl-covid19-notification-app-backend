// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2


//Do not delete.

//public class EksJobContentWriterMk2 : IEksJobContentWriter
//{
//    private readonly Func<ContentDbContext> _ContentDbContext;
//    private readonly Func<PublishingJobDbContext> _PublishingDbContext;
//    private readonly IPublishingIdService _PublishingIdService;
//    private readonly ILogger<EksJobContentWriterMk2> _Logger;
//    private readonly IGaContentSigner _GaContentSigner;
//    private readonly IContentSigner _Nlv1ContentSigner;
//    private readonly IContentSigner _Nlv2ContentSigner;

//    private string _PublishingId;
//    private byte[] _GaenSig;
//    private EksCreateJobOutputEntity _Current;

//    public EksJobContentWriterMk2(Func<ContentDbContext> contentDbContext, Func<PublishingJobDbContext> publishingDbContext, IPublishingIdService publishingIdService, ILogger<EksJobContentWriterMk2> logger, IGaContentSigner gaContentSigner, IContentSigner nlv1ContentSigner, IContentSigner nlv2ContentSigner)
//    {
//        _ContentDbContext = contentDbContext ?? throw new ArgumentNullException(nameof(contentDbContext));
//        _PublishingDbContext = publishingDbContext ?? throw new ArgumentNullException(nameof(publishingDbContext));
//        _PublishingIdService = publishingIdService ?? throw new ArgumentNullException(nameof(publishingIdService));
//        _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
//        _GaContentSigner = gaContentSigner ?? throw new ArgumentNullException(nameof(gaContentSigner));
//        _Nlv1ContentSigner = nlv1ContentSigner ?? throw new ArgumentNullException(nameof(nlv1ContentSigner));
//        _Nlv2ContentSigner = nlv2ContentSigner ?? throw new ArgumentNullException(nameof(nlv2ContentSigner));
//    }

//    public async Task ExecuteAsyc()
//    {
//        await using var pdbc = _PublishingDbContext();
//        await using (pdbc.BeginTransaction()) //Read consistency
//        foreach (var i in pdbc.EksOutput)
//            await Process(i);
//    }

//    private async Task Process(EksCreateJobOutputEntity i)
//    {
//        _Current = i;
//        _PublishingId = _PublishingIdService.Create(i.Content);
//        _GaenSig = _GaContentSigner.GetSignature(i.Content);
//        await Write(ContentTypes.ExposureKeySet, _Nlv1ContentSigner);
//        await Write(ContentTypes.ExposureKeySetV2, _Nlv2ContentSigner);
//    }

//    private async Task Write(string type, IContentSigner nlContentSigner)
//    {
//        var nlSig = nlContentSigner.GetSignature(_Current.Content);
//        var value = await new ZippedContentBuilder().BuildEks(_Current.Content, _GaenSig, nlSig);
//        await WriteContent(value, type);
//    }

//    private async Task WriteContent(byte[] contentValue, string type)
//    {
//        var e = new ContentEntity
//        {
//            Created = _Current.Release,
//            Release = _Current.Release,
//            ContentTypeName = MediaTypeNames.Application.Zip,
//            Content = contentValue,
//            Type = type,
//            PublishingId = _PublishingId
//        };
//        await using var cdbc = _ContentDbContext();
//        await using (cdbc.BeginTransaction())
//        {
//            await cdbc.Content.AddAsync(e);
//            cdbc.SaveAndCommit();
//        }
//    }
//}

//public class EksGaenSignatureBuilder
//{
//    private readonly GaenSignatureInfoBuilder _GaenSignatureInfoBuilder;
//    private readonly IGaContentSigner _GaenContentSigner;
//    private readonly IEksContentFormatter _EksContentFormatter;

//    public EksGaenSignatureBuilder(GaenSignatureInfoBuilder gaenSignatureInfoBuilder, IGaContentSigner gaenContentSigner, IEksContentFormatter eksContentFormatter)
//    {
//        _GaenSignatureInfoBuilder = gaenSignatureInfoBuilder ?? throw new ArgumentNullException(nameof(gaenSignatureInfoBuilder));
//        _GaenContentSigner = gaenContentSigner ?? throw new ArgumentNullException(nameof(gaenContentSigner));
//        _EksContentFormatter = eksContentFormatter ?? throw new ArgumentNullException(nameof(eksContentFormatter));
//    }

//    public byte[] Build(byte[] content)
//    {
//        var securityInfo = _GaenSignatureInfoBuilder.Build();
//        var gaenSig = _GaenContentSigner.GetSignature(content);
//        var signatures = new ExposureKeySetSignaturesContentArgs
//        {
//            Items = new[]
//            {
//                new ExposureKeySetSignatureContentArgs
//                {
//                    SignatureInfo = securityInfo,
//                    Signature = gaenSig,
//                    BatchNum = 1,
//                    BatchSize = 1,
//                },
//            }
//        };

//        return _EksContentFormatter.GetBytes(signatures);
//    }
//}
//public class EksContentBuilderV1 : IEksBuilder
//{
//    private const string Header = "EK Export v1    ";
//    private readonly GaenSignatureInfoBuilder _GaenSignatureInfoBuilder;
//    private readonly IUtcDateTimeProvider _DateTimeProvider;
//    private readonly IEksContentFormatter _EksContentFormatter;

//    public EksContentBuilderV1(GaenSignatureInfoBuilder gaenSignatureInfoBuilder, IUtcDateTimeProvider dateTimeProvider, IEksContentFormatter eksContentFormatter)
//    {
//        _GaenSignatureInfoBuilder = gaenSignatureInfoBuilder ?? throw new ArgumentNullException(nameof(gaenSignatureInfoBuilder));
//        _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
//        _EksContentFormatter = eksContentFormatter ?? throw new ArgumentNullException(nameof(eksContentFormatter));
//    }

//    public async Task<byte[]> BuildAsync(TemporaryExposureKeyArgs[] keys)
//    {
//        var securityInfo = _GaenSignatureInfoBuilder.Build();

//        if (keys == null) throw new ArgumentNullException(nameof(keys));
//        if (keys.Any(x => x == null)) throw new ArgumentException("At least one key in null.", nameof(keys));

//        var content = new ExposureKeySetContentArgs
//        {
//            Header = Header,
//            Region = "NL",
//            BatchNum = 1,
//            BatchSize = 1,
//            SignatureInfos = new[] {securityInfo},
//            StartTimestamp = _DateTimeProvider.Snapshot.AddDays(-1).ToUnixTimeU64(),
//            EndTimestamp = _DateTimeProvider.Snapshot.ToUnixTimeU64(),
//            Keys = keys
//        };

//        return _EksContentFormatter.GetBytes(content);
//    }
//}
//public class GaenSignatureInfoBuilder
//{
//    private readonly IGaContentSigner _GaenContentSigner;
//    private readonly IEksHeaderInfoConfig _Config;

//    public GaenSignatureInfoBuilder(IGaContentSigner gaenContentSigner, IEksHeaderInfoConfig config)
//    {
//        _GaenContentSigner = gaenContentSigner ?? throw new ArgumentNullException(nameof(gaenContentSigner));
//        _Config = config ?? throw new ArgumentNullException(nameof(config));
//    }

//    public SignatureInfoArgs Build() => new SignatureInfoArgs
//    {
//        SignatureAlgorithm = _GaenContentSigner.SignatureOid,
//        VerificationKeyId = _Config.VerificationKeyId,
//        VerificationKeyVersion = _Config.VerificationKeyVersion,
//        AppBundleId = _Config.AppBundleId
//    };
//}