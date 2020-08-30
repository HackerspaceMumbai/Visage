var assert = require("assert");
var oxford = require('project-oxford');
var meetupFaceManager = require('../src-server/MeetupFaceManager.js');
var returnValue = null;
const MEETUP_NAME = "mochatestmeetupevent";
const MEETUP_ATTENDEE_NAME = "test@outlook.com";
client = new oxford.Client(process.env.FACE_API_KEY);
describe('MeetupFaceManager-EventExists-Tests', function () {
    describe('MeetupFaceManager-EventExists-Tests-EventDoesNotExists', function () {
        before(function () {
            this.timeout(10000);
            return client.face.personGroup.delete(MEETUP_NAME);
        });
        it('Assert  Meetup/Event does not exists', function (done) {
            this.timeout(10000);
            meetupFaceManager.EventExists(MEETUP_NAME)
                .then(function (response) {
                console.log("Event Meetup does not exist REsponse : " + response.EventExists);
                assert.equal(false, response.EventExists);
                done();
            })
                .catch(function (err) {
                done(err);
            });
        });
    });
    describe('MeetupFaceManager-EventExists-Tests-EventExists', function () {
        before(function () {
            this.timeout(10000);
            return client.face.personGroup.delete(MEETUP_NAME)
                .then(function (response) {
                console.log("Meetup Cleaned Up");
                return client.face.personGroup.create(MEETUP_NAME, MEETUP_NAME, MEETUP_NAME);
            })
                .catch(function (err) {
                console.log("No Meetup to Cleanup --");
                return client.face.personGroup.create(MEETUP_NAME, MEETUP_NAME, MEETUP_NAME);
            });
        });
        it('Assert  Meetup exists', function (done) {
            this.timeout(10000);
            meetupFaceManager.EventExists(MEETUP_NAME)
                .then(function (response) {
                console.log("Event Meetup exists REsponse : " + response.EventExists);
                assert.equal(true, response.EventExists);
                done();
            })
                .catch(function (err) {
                done(err);
            });
        });
    });
});
//# sourceMappingURL=MeetupFaceManagerEventExistsTest.js.map