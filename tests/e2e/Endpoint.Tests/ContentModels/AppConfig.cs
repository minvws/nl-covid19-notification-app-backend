// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace Endpoint.Tests.ContentModels
{
    public class AppConfig
    {
        public int androidMinimumVersion { get; set; }
        public string appointmentPhoneNumber { get; set; }
        public int decoyProbability { get; set; }
        public string iOSAppStoreURL { get; set; }
        public string iOSMinimumVersion { get; set; }
        public string iOSMinimumVersionMessage { get; set; }
        public int manifestFrequency { get; set; }
        public int repeatedUploadDelay { get; set; }
        public int requestMaximumSize { get; set; }
        public int requestMinimumSize { get; set; }
        public string shareKeyURL { get; set; }
    }
}
