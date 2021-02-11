// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

import { IAppConfig } from './app-config.service';

export interface IAppConfigService {
    loadAppConfig();

    getConfig(): IAppConfig;
}
