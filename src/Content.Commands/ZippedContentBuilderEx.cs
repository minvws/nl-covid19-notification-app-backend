// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    public static class ZippedContentBuilderEx
    {
        public static async Task<byte[]> BuildEksAsync(
            this ZippedContentBuilder thiz,
            byte[] content,
            byte[] gaenSignature,
            byte[] cmsSignature)
        {
            var args = new[]
            {
                new ZippedContentBuilderArgs { Value = content, EntryName = ZippedContentEntryNames.EksContent},
                new ZippedContentBuilderArgs { Value = gaenSignature, EntryName = ZippedContentEntryNames.EksGaenSig},
                new ZippedContentBuilderArgs { Value = cmsSignature, EntryName = ZippedContentEntryNames.CmsSignature},
            };

            return await thiz.BuildAsync(args);
        }


        public static async Task<byte[]> BuildStandardAsync(
            this ZippedContentBuilder thiz,
            byte[] content,
            byte[] cmsSignature)
        {
            var args = new[]
            {
                new ZippedContentBuilderArgs { Value = content, EntryName = ZippedContentEntryNames.Content},
                new ZippedContentBuilderArgs { Value = cmsSignature, EntryName = ZippedContentEntryNames.CmsSignature},
            };

            return await thiz.BuildAsync(args);
        }
    }
}
