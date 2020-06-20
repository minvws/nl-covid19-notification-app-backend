// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret
{
    public class HttpPostRegisterSecret
    {
        private readonly ISecretWriter _Writer;

        public HttpPostRegisterSecret(ISecretWriter writer)
        {
            _Writer = writer;
        }

        public async Task<EnrollmentResponse> Execute()
        {
            return await _Writer.Execute();
        }
    }
}