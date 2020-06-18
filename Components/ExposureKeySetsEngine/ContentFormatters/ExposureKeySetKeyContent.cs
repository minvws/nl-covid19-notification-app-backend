// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using ProtoBuf;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.ContentFormatters
{
    [ProtoContract]
    public class ExposureKeySetKeyContent
    {
        [ProtoMember(1)]
        public byte[] DailyKey { get; set; }

        [ProtoMember(2)]
        public int RollingStart { get; set; }

        [ProtoMember(3)]
        public int RollingPeriod { get; set; }

        [ProtoMember(4)]
        public int Risk { get; set; }
    }
}