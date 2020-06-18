// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using ProtoBuf;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.ContentFormatters
{
    [ProtoContract]
    public class ExposureKeySetContent
    {
        [ProtoMember(1)]
        public string Header { get; set; }
        [ProtoMember(2)]
        public string Region { get; set; }
        [ProtoMember(3)]
        public int BatchNum { get; set; }
        [ProtoMember(4)]
        public int BatchSize { get; set; }
        [ProtoMember(5)]
        public ulong StartTimestamp { get; set; }
        [ProtoMember(6)]
        public ulong EndTimestamp { get; set; }
        [ProtoMember(7)]
        public SignatureInfo[] SignatureInfos { get; set; }
        [ProtoMember(8)]
        public ExposureKeySetKeyContent[] Keys { get; set; }
    }
}