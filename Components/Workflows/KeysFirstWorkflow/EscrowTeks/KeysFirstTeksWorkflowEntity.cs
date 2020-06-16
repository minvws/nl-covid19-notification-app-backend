// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.ComponentModel.DataAnnotations.Schema;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.EscrowTeks
{
    public class KeysFirstTeksWorkflowEntity
    {
        public int Id { get; set; }

        public DateTime Created { get; set; }

        public string AuthorisationToken { get; set; }

        public bool Authorised { get; set; }

        /// <summary>
        /// TEK[] as JSON
        /// </summary>
        public string TekContent { get; set; }

        public string Region { get; set; } = DefaultValues.Region;


    }
}