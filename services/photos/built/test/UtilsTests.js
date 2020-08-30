const assert = require('assert');
const utils = require('../src-server/Utils.js');
function ResetAllEnvVariables() {
    delete process.env.EVENT_ID_VARIABLE_NAME;
    delete process.env.PARTICIPANT_ID_VARIABLE_NAME;
    delete process.env.LOGIN_WITH_SERVICE_URI;
    delete process.env.APP_DOMAIN_NAME;
}
const NON_DEFAULT_EVENT_PARAM_NAME = "NotDefaultEvent";
const NON_DEFAULT_PARTICIPANT_PARAM_NAME = "NotDefaultEvent";
const PROFILE_COOKIE = "%7b%22username%22%3a%221110311111%22%2c%22name%22%3a%22Mani+Bindra%22%2c%22provider%22%3a%22eventbrite%22%7d";
const PARTICIPANT_ID = "1110311111";
describe('Utilitily-Tests', function () {
    describe('Get Event Id Parameter Name - Parameter Name configured', function () {
        before(function () {
            ResetAllEnvVariables();
            process.env.EVENT_ID_VARIABLE_NAME = NON_DEFAULT_EVENT_PARAM_NAME;
        });
        it('Non Default Event Parameter Name configured', function (done) {
            assert.equal(utils.GetEventIdUrlParamName(), NON_DEFAULT_EVENT_PARAM_NAME);
            done();
        });
    });
    describe('Get Event Id Parameter Name - Parameter Name not configured', function () {
        before(function () {
            ResetAllEnvVariables();
        });
        it(' Event Parameter Name not configured', function (done) {
            assert.equal(utils.GetEventIdUrlParamName(), utils.DEFAULT_EVENT_PARAMETER_NAME);
            done();
        });
    });
    describe('Get Participant Id Parameter Name - Parameter Name configured', function () {
        before(function () {
            ResetAllEnvVariables();
            process.env.PARTICIPANT_ID_VARIABLE_NAME = NON_DEFAULT_PARTICIPANT_PARAM_NAME;
        });
        it('Non Default Participant Parameter Name configured', function (done) {
            assert.equal(utils.GetParticipantIdUrlParamName(), NON_DEFAULT_PARTICIPANT_PARAM_NAME);
            done();
        });
    });
    describe('Get Participant ID Parameter Name - Parameter Name not configured', function () {
        before(function () {
            ResetAllEnvVariables();
        });
        it('Participant ID Parameter Name not configured', function (done) {
            assert.equal(utils.GetParticipantIdUrlParamName(), utils.DEFAULT_PARTICIPANT_PARAMETER_NAME);
            done();
        });
    });
    describe('ValueExists Tests', function () {
        before(function () {
            ResetAllEnvVariables();
            process.env.PARTICIPANT_ID_VARIABLE_NAME = NON_DEFAULT_PARTICIPANT_PARAM_NAME;
        });
        it('Value Exists and Not Exists assertions', function (done) {
            assert.equal(true, utils.ValueExists(process.env.PARTICIPANT_ID_VARIABLE_NAME));
            assert.equal(false, utils.ValueExists(process.env.EVENT_ID_VARIABLE_NAME));
            done();
        });
    });
    describe('Get Participant Id from Profile Cookie tests', function () {
        before(function () {
            ResetAllEnvVariables();
        });
        it('Get Value from profile Cookie', function (done) {
            assert.equal(PARTICIPANT_ID, utils.GetProfileIdFromProfileCookie(PROFILE_COOKIE));
            done();
            ResetAllEnvVariables();
        });
    });
});
//# sourceMappingURL=UtilsTests.js.map