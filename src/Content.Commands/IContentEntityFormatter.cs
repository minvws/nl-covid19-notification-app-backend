// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    public interface IContentEntityFormatter
    {
        Task<TContentEntity> FillAsync<TContentEntity, TContent>(TContentEntity e, TContent c) where TContentEntity : ContentEntity;
    }
}