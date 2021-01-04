// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.ComponentModel.DataAnnotations.Schema;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Entities
{
    public class DiagnosisKeyEntity
    {
        /// <summary>
        /// </summary>
        public bool PublishedLocally { get; set; }


        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public DailyKey DailyKey { get; set; } = new DailyKey();
        public TekOrigin Origin { get; set; }
        public LocalTekInfo Local { get; set; } = new LocalTekInfo();
        
        /// <summary>
        /// Immediately set to true for DKs imported from EFGS
        /// </summary>
        public bool PublishedToEfgs { get; set; } //TODO this is unused until EFGS is 
        public EfgsTekInfo Efgs { get; set; } = new EfgsTekInfo(); //TODO this is unused until EFGS is 
    }
}