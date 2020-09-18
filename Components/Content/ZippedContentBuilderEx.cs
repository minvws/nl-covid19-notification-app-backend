// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public static class ZippedContentBuilderEx
    {
        public static async Task<byte[]> BuildEks(this ZippedContentBuilder thiz, byte[] content, byte[] gaenSig, byte[] nlSig)
        {
            var args = new[]
            {
                new ZippedContentBuilderArgs { Value = content, EntryName = ZippedContentEntryNames.EksContent},
                new ZippedContentBuilderArgs { Value = gaenSig, EntryName = ZippedContentEntryNames.EksGaenSig},
                new ZippedContentBuilderArgs { Value = nlSig, EntryName = ZippedContentEntryNames.NLSignature},
            };

            return await thiz.Build(args);
        }

        
        public static async Task<byte[]> BuildStandard(this ZippedContentBuilder thiz, byte[] content, byte[] nlSig)
        {
            var args = new[]
            {
                new ZippedContentBuilderArgs { Value = content, EntryName = ZippedContentEntryNames.Content},
                new ZippedContentBuilderArgs { Value = nlSig, EntryName = ZippedContentEntryNames.NLSignature},
            };

            return await thiz.Build(args);
        }
    }
}