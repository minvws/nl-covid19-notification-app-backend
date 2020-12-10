// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities
{
    public enum PublishingState
    {
        Unpublished,
        //Processing, //NB not needed cos job table.
        Published
    }
}