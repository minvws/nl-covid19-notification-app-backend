// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

import { User } from '../models';
import {AuthenticationService} from './authentication.service';

export class AuthenticationTestService extends AuthenticationService {
    public get currentUserValue(): User {
        return {
            id: 0,
            email: 'test@test.test',
            displayName: 'Test User',
            authData: 'authData'
        };
    }
}
