// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret
{
    public interface IRandomNumberGenerator
    {
        string GenerateToken(int length = 6);

        int Next(int min, int max);

        byte[] GenerateKey(int keyLength = 32);
    }
}