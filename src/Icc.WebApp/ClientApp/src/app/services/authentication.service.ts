// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

import { Injectable } from '@angular/core';
import {HttpClient, HttpErrorResponse} from '@angular/common/http';
import {BehaviorSubject, Observable, of} from 'rxjs';
import {catchError, map} from 'rxjs/operators';
import {User} from '../models';
import {JwtHelperService} from '@auth0/angular-jwt';
import {AppConfigService} from './app-config.service';
import {Router} from '@angular/router';

@Injectable({providedIn: 'root'})
export class AuthenticationService {
    private currentUserSubject: BehaviorSubject<User>;
    public currentUser: Observable<User>;

    constructor(private readonly http: HttpClient, private readonly router: Router, private readonly appConfigService: AppConfigService) {
        const jwtToken = localStorage.getItem('auth');
        const decodedToken = AuthenticationService.decodeToken({jwtToken: localStorage.getItem('auth')});
        this.currentUserSubject = new BehaviorSubject<User>(AuthenticationService.parsePayloadToUser(jwtToken, decodedToken));
        this.currentUser = this.currentUserSubject.asObservable();
    }

    private static decodeToken({jwtToken, helper = null}: { jwtToken: string; helper?: JwtHelperService; }): object {
        helper = (helper) ? helper : new JwtHelperService();
        if (!helper.isTokenExpired(jwtToken)) {
            return helper.decodeToken(jwtToken);
        }
        return null;
    }

    private static parsePayloadToUser(jwtToken: string, payload: any): User {
        if (payload && payload.id) {

            return {
                displayName: payload.name,
                email: payload.email,
                id: payload.id,
                authData: jwtToken
            };
        }
        return null;
    }

    private static errorHandler(error: HttpErrorResponse, caught: Observable<any>): Observable<any> {
        // TODO error handling
        throw error;
    }

  public buildUrl(endpoint: string) {
    return 'https://' + this.appConfigService.getConfig().authHost + (endpoint.startsWith('/') ? '' : '/') + endpoint;
    }

    public fetchCurrentUser(): Observable<any> {
        const serviceUrl = this.buildUrl('Auth/User');
        const headers = {
            headers: {
                'Authorization': 'Bearer ' + ((this.currentUserValue !== null) ? this.currentUserValue.authData : '')
            }
        };
        return this.http.get<any>(serviceUrl, headers).pipe(
            map(response => {
                if (response.user.id === this.currentUserValue.id) {
                    return true;
                }
                return this.router.parseUrl('');
            }),
            catchError(error => {
                this.logout();
                return of(false);
            })
        );
    }

    public get currentUserValue(): User {
        return this.currentUserSubject.value;
    }

    public callback(authorizationCode: string): Observable<boolean> {
        const serviceUrl = this.buildUrl('Auth/Token');

        return this.http.post<any>(serviceUrl, {code: authorizationCode})
            .pipe(
                map(response => {
                    const jwtToken = response.token;
                    const helper = new JwtHelperService();
                    if (!helper.isTokenExpired(jwtToken)) {
                        const decodedToken = AuthenticationService.decodeToken({jwtToken});
                        this.currentUserSubject.next(AuthenticationService.parsePayloadToUser(jwtToken, decodedToken));
                        localStorage.setItem('auth', jwtToken);
                        return true;
                    }
                    return false;
                }),
                catchError(AuthenticationService.errorHandler)
            );
    }

    public logout(redirect = true) {
        // remove user from local storage to log user out
        localStorage.removeItem('auth');
        this.currentUserSubject.next(null);
        if (redirect) {
            window.location.href = this.buildUrl('Auth/Logout');
        }
    }

    public redirectToAuthorization() {
        window.location.href = this.buildUrl('Auth/Redirect');
    }
}

