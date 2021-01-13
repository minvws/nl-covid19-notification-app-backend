import {User} from '../models';
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
