// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ICC
{
    public class InfectionConfirmationCodeEntity
    {
        public int Id { get; set; }
        
        /// <summary>
        /// 32-digit Infection Confirmation Code
        /// </summary>
        public string Code { get; set; } 
        
        public DateTime Created { get; set; }
    }
}