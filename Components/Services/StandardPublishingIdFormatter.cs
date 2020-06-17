// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Globalization;
using System.Text;
using System.Web;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services
{
    public class StandardPublishingIdFormatter : IPublishingId
    {
        private readonly ISigner _Signer;

        public StandardPublishingIdFormatter(ISigner signer)
        {
            _Signer = signer;
        }

        public string Create(byte[] contents)
        {
            var id = _Signer.GetSignature(contents);
            var result = new StringBuilder(id.Length * 2);
            foreach (var i in id)
                result.AppendFormat("{0:x2}", i);

            return result.ToString();
        }

        [Obsolete("Not sure this are a problem now")]
        public string ParseUri(string uri)
            => HttpUtility.UrlDecode(uri).Replace(" ", "+");

        public bool Validate(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            if(id.Length != _Signer.LengthBytes * 2)
                return false;

            for (var i = 0; i < id.Length; i += 2)
            {
                if (!int.TryParse(id.Substring(i, 2), NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out _))
                    return false;
            }

            return true;
        }
    }
}