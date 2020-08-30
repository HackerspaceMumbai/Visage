const assert = require('assert');
const authManager = require('../src-server/AuthenticationManager.js');
const utils = require('../src-server/Utils.js');


function ResetAllEnvVariables() {
    delete process.env.EVENT_ID_VARIABLE_NAME;
    delete process.env.PARTICIPANT_ID_VARIABLE_NAME;
    delete process.env.LOGIN_WITH_SERVICE_URI;
    delete process.env.APP_DOMAIN_NAME;

}

const LOGIN_WITH_SERVICE_URI = "https://login.testdomain.com";
const APP_DOMAIN_NAME = "https://www.testdomain.com";
const EVENT_NAME = "acsmeetup";
const PARTICIPANT_ID = "2341123423";
const EXTERNAL_AUTHENTICATION_URL = "https://login.testdomain.com/eventbrite?success=https://www.testdomain.com/success/acsmeetup&failure=https://www.testdomain.com/failure";
const PROFILE_COOKIE = "%7b%22username%22%3a%221110311111%22%2c%22name%22%3a%22Mani+Bindra%22%2c%22provider%22%3a%22eventbrite%22%7d";
const POST_AUTH_REDIRECTION_URL = "https://www.testdomain.com/?event_name=acsmeetup&participant_email=1110311111";


describe('Authentication-Tests', function () {


    describe('Application Not Configured for External Authentication Tests', function () {
        before(function () {
            ResetAllEnvVariables();
        });

        it('Application not configured for external Authentication - 1', function (done) {
            assert.equal(false, authManager.GetParticipantProfileByOAuth());
            done();
        });
        it('Application not configured for external Authentication - 2', function (done) {
            ResetAllEnvVariables();
            process.env.LOGIN_WITH_SERVICE_URI = LOGIN_WITH_SERVICE_URI;
            assert.equal(false, authManager.GetParticipantProfileByOAuth());
            done();
        });
        it('Application not configured for external Authentication - 3', function (done) {
            ResetAllEnvVariables();
            process.env.APP_DOMAIN_NAME = APP_DOMAIN_NAME;
            assert.equal(false, authManager.GetParticipantProfileByOAuth());
            done();
        });
    });

    describe('Application Configured for External Authentication Tests', function () {
        before(function () {
            ResetAllEnvVariables();
            process.env.LOGIN_WITH_SERVICE_URI = LOGIN_WITH_SERVICE_URI;
            process.env.APP_DOMAIN_NAME = APP_DOMAIN_NAME;
        });

        it('Application configured for external Authentication', function (done) {
            assert.equal(true, authManager.GetParticipantProfileByOAuth());
            done();
        });
    });

    describe('GetExternalAuthenticationUrl tests', function () {
        before(function () {
            ResetAllEnvVariables();
            process.env.LOGIN_WITH_SERVICE_URI = LOGIN_WITH_SERVICE_URI;
            process.env.APP_DOMAIN_NAME = APP_DOMAIN_NAME;
        });

        it('Assert External Authentication URL is correct', function (done) {
            assert.equal(EXTERNAL_AUTHENTICATION_URL, authManager.GetExternalAuthenticationUrl(EVENT_NAME));
            done();
        });
    });


    describe('GetPostAuthenticationSuccessRedirectionUrl-Tests', function () {
        before(function () {
            ResetAllEnvVariables();
            process.env.LOGIN_WITH_SERVICE_URI = LOGIN_WITH_SERVICE_URI;
            process.env.APP_DOMAIN_NAME = APP_DOMAIN_NAME;

        });

        it('Assert Post Authentication Success Redirection Url is correct', function (done) {
            assert.equal(POST_AUTH_REDIRECTION_URL, authManager.GetPostAuthenticationSuccessRedirectionUrl(PROFILE_COOKIE, EVENT_NAME));
            done();
        });
    });

        describe('GetNextAction-Tests', function () {
        before(function () {
            ResetAllEnvVariables();
        });


        it('Next Action BAD Request - no configurations', function (done) {
            var profileCookie ;
            var externalAuthentication ;
            var eventIdParameterValue ;
            var participantIdParameterValue ;
            var response = authManager.GetNextAction(profileCookie,externalAuthentication,eventIdParameterValue, participantIdParameterValue);
           assert.equal(true, utils.ActionType.SHOW_BAD_REQUEST_MESSAGE === response.actionType);
            done();
        });

        it('Next Action BAD Request - external authentication but no event name', function (done) {
            var profileCookie ;
            var externalAuthentication = true ;
            var eventIdParameterValue ;
            var participantIdParameterValue ;
            var response = authManager.GetNextAction(profileCookie,externalAuthentication,eventIdParameterValue, participantIdParameterValue);
           assert.equal(true, utils.ActionType.SHOW_BAD_REQUEST_MESSAGE === response.actionType  );
            done();
        });

         it('Next Action Show upload Screen', function (done) {
            var profileCookie ;
            var externalAuthentication = false ;
            var eventIdParameterValue = EVENT_NAME;
            var participantIdParameterValue = PARTICIPANT_ID;
            var response = authManager.GetNextAction(profileCookie,externalAuthentication,eventIdParameterValue, participantIdParameterValue);
           assert.equal(true, utils.ActionType.RENDER_DEFAULT_FILE === response.actionType);
            done();
        });

        it('Next Action Show image upload screen', function (done) {
            var profileCookie = PROFILE_COOKIE;
            var externalAuthentication = false ;
            var eventIdParameterValue = EVENT_NAME;
            var participantIdParameterValue = PARTICIPANT_ID;
            var response = authManager.GetNextAction(profileCookie,externalAuthentication,eventIdParameterValue, participantIdParameterValue);
           assert.equal(true, utils.ActionType.RENDER_DEFAULT_FILE === response.actionType);
            done();
        });

        it('Next Action Show image upload screen', function (done) {
            var profileCookie = PROFILE_COOKIE;
            var externalAuthentication = true ;
            var eventIdParameterValue = EVENT_NAME;
            var participantIdParameterValue = PARTICIPANT_ID;
            var response = authManager.GetNextAction(profileCookie,externalAuthentication,eventIdParameterValue, participantIdParameterValue);
           assert.equal(true, utils.ActionType.RENDER_DEFAULT_FILE === response.actionType);
            done();
        });

        it('Next Action Redirect to sucess to fetch participat id from cookie', function (done) {
            var profileCookie = PROFILE_COOKIE;
            var externalAuthentication = true ;
            var eventIdParameterValue = EVENT_NAME;
            var participantIdParameterValue ;
            var response = authManager.GetNextAction(profileCookie,externalAuthentication,eventIdParameterValue, participantIdParameterValue);
           assert.equal(true, utils.ActionType.REDIRECT_SUCCESS === response.actionType);
            done();
        });

        it('Next Action Initiate external Authentication Oauth - 1, no participant , no profile', function (done) {
            var profileCookie;
            var externalAuthentication = true ;
            var eventIdParameterValue = EVENT_NAME;
            var participantIdParameterValue ;
            var response = authManager.GetNextAction(profileCookie,externalAuthentication,eventIdParameterValue, participantIdParameterValue);
           assert.equal(true, utils.ActionType.REDIRECT_OAUTH === response.actionType);
            done();
        });

        it('Next Action Initiate external Authentication Oauth - 2 no profile cookie, all other values set', function (done) {
            var profileCookie;
            var externalAuthentication = true ;
            var eventIdParameterValue = EVENT_NAME;
            var participantIdParameterValue = PARTICIPANT_ID ;
            var response = authManager.GetNextAction(profileCookie,externalAuthentication,eventIdParameterValue, participantIdParameterValue);
           assert.equal(true, utils.ActionType.REDIRECT_OAUTH === response.actionType);
            done();
        });

            it('Next Action Bad request - external authentication and profile cookie but no event name', function (done) {
            var profileCookie = PROFILE_COOKIE;
            var externalAuthentication = true ;
            var eventIdParameterValue;
            var participantIdParameterValue ;
            var response = authManager.GetNextAction(profileCookie,externalAuthentication,eventIdParameterValue, participantIdParameterValue);
           assert.equal(true, utils.ActionType.SHOW_BAD_REQUEST_MESSAGE === response.actionType);
            done();
        });


    });

});