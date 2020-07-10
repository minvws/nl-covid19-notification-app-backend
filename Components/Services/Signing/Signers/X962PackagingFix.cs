// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers
{
    /// <summary>
    /// Packaging fix over MS .NET implementation
    /// Converted from C provided by DWvG.
    /// </summary>
    public class X962PackagingFix
    {
        public byte[] Format(byte[] value)
        {
            var p = new byte[32];
            var r = new byte[32];
            Array.Copy(value, p, 32);
            Array.Copy(value, 32, r, 0, 32);
            var buffer = new byte[72];
            buffer[0] = 0x30;
            buffer[1] = (byte)(0x44 + (p[0] >= 0x80 ? 1 : 0) + (r[0] >= 0x80 ? 1 : 0));
            var ia = new byte[] { 0x02, 0x20 };
            var ib = new byte[] { 0x02, 0x21, 0x0 };
            var index = 2;
            if (p[0] >= 0x80)
            {
                Array.Copy(ib, 0, buffer, index, ib.Length);
                index += ib.Length;
            }
            else
            {
                Array.Copy(ia, 0, buffer, index, ia.Length);
                index += ia.Length;
            }
            Array.Copy(p, 0, buffer, index, p.Length);
            index += p.Length;
            if (r[0] >= 0x80)
            {
                Array.Copy(ib, 0, buffer, index, ib.Length);
                index += ib.Length;
            }
            else
            {
                Array.Copy(ia, 0, buffer, index, ia.Length);
                index += ia.Length;
            }
            Array.Copy(r, 0, buffer, index, r.Length);
            index += r.Length;
            var result = new byte[index];
            Array.Copy(buffer, result, index);
            return result;
        }
    }
}