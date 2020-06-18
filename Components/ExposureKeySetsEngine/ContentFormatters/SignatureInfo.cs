// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using ProtoBuf;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.ContentFormatters
{
    [ProtoContract]
    public class SignatureInfo
    {
        [ProtoMember(1)]
        public string AppBundleId { get; set; }
        [ProtoMember(2)]
        public string AndroidPackage { get; set; }
        [ProtoMember(3)]
        public string SignatureAlgorithm { get; set; }
        [ProtoMember(4)]
        public string VerificationKeyId { get; set; }
        [ProtoMember(5)]
        public string VerificationKeyVersion { get; set; }
    }
}