// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Models
{
    static class TheIdentityHubClaimTypes
    {
        internal const string AccessToken = "http://schemas.u2uconsult.com/ws/2014/03/identity/claims/accesstoken";
        internal const string AuthenticationStrength = "http://schemas.u2uconsult.com/ws/2016/08/identity/claims/authenticationstrength";
        internal const string ClientId = "http://schemas.u2uconsult.com/ws/2014/11/identity/claims/clientid";
        internal const string OldIdentityId = "http://schemas.u2uconsult.com/ws/2019/02/identity/claims/oldidentityid";
        internal const string DisplayName = "http://schemas.u2uconsult.com/ws/2014/04/identity/claims/displayname";
        internal const string EmailAddress = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
        internal const string EmailAddressVerified = "http://schemas.u2uconsult.com/ws/2017/02/identity/claims/emailaddressverified";
        internal const string GivenName = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname";
        internal const string IdentityProvider = "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider";
        internal const string LargePicture = "http://schemas.u2uconsult.com/ws/2014/04/identity/claims/largepicture";
        internal const string MediumPicture = "http://schemas.u2uconsult.com/ws/2014/04/identity/claims/mediumpicture";
        internal const string Scope = "http://schemas.u2uconsult.com/ws/2014/03/identity/claims/scope";
        internal const string Surname = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname";
        internal static readonly string NameIdentifier = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
    }
}