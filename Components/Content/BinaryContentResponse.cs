// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using ProtoBuf;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    [ProtoContract]
    public class BinaryContentResponse
    {
        [ProtoMember(1)]
        public string PublishingId { get; set; }
        [ProtoMember(2)]
        public DateTime LastModified { get; set; }
        [ProtoMember(3)]
        public string? ContentTypeName { get; set; }
        [ProtoMember(4)]
        public byte[]? Content { get; set; }
        [ProtoMember(5)]
        public string? SignedContentTypeName { get; set; }
        [ProtoMember(6)]
        public byte[]? SignedContent { get; set; }
    }

    public class ReceiveContentArgs
    {
        public string PublishingId { get; set; }
        public DateTime LastModified { get; set; }
        public byte[]? SignedContent { get; set; }
    }
}
