var assert = require("assert");
var oxford = require('project-oxford');
var should = require('should');
var meetupFaceManager = require('../src-server/MeetupFaceManager.js');
var returnValue = null;
const MEETUP_NAME = "mochatestmeetupattendee";
const MEETUP_ATTENDEE_NAME = "test@outlook.com";
client = new oxford.Client(process.env.FACE_API_KEY);
describe('MeetupFaceManager-AttendeeExists-Tests', function () {
    describe('MeetupFaceManager-AttendeeExists-Tests-AttendeeDoesNotExist-MeetupExists', function () {
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
        it('Assert Attendee Does Not Exist, Meetup Exists', function (done) {
            this.timeout(10000);
            meetupFaceManager.AttendeeExists(MEETUP_NAME, MEETUP_ATTENDEE_NAME)
                .then(function (response) {
                console.log("Attendee does not exist, Meetup Exists REsponse : " + response.PersonExists);
                assert.equal(false, response.PersonExists);
                done();
            })
                .catch(function (err) {
                done(err);
            });
        });
    });
    describe('MeetupFaceManager-AttendeeExists-Tests-MeetupDoesNotExist-AttendeeDoesNotExist', function () {
        before(function () {
            this.timeout(10000);
            return client.face.personGroup.delete(MEETUP_NAME);
        });
        it('Assert Attendee Does Not Exist, and Meetup Does not exist', function (done) {
            this.timeout(10000);
            meetupFaceManager.AttendeeExists(MEETUP_NAME, MEETUP_ATTENDEE_NAME)
                .then(function (response) {
                console.log("Attendee does not exist, and meetup Does not exist REsponse : " + response.PersonExists);
                assert.equal(false, response.PersonExists);
                done();
            })
                .catch(function (err) {
                done(err);
            });
        });
    });
    // Following Test Also tests Add Person
    describe('MeetupFaceManager-AttendeeExists-Tests-AttendeeExists-AndHasPersonId', function () {
        before(function () {
            this.timeout(10000);
            return client.face.personGroup.delete(MEETUP_NAME)
                .then(function (response) {
                console.log("Meetup Cleaned Up");
                return client.face.personGroup.create(MEETUP_NAME, MEETUP_NAME, MEETUP_NAME)
                    .then(function (response) {
                    return meetupFaceManager.AddPerson(MEETUP_NAME, MEETUP_ATTENDEE_NAME);
                });
            })
                .catch(function (err) {
                console.log("No Meetup to Cleanup --");
                return client.face.personGroup.create(MEETUP_NAME, MEETUP_NAME, MEETUP_NAME)
                    .then(function (response) {
                    return meetupFaceManager.AddPerson(MEETUP_NAME, MEETUP_ATTENDEE_NAME);
                });
            });
        });
        it('Assert  Attendee exists and has property PersonId', function (done) {
            this.timeout(10000);
            meetupFaceManager.AttendeeExists(MEETUP_NAME, MEETUP_ATTENDEE_NAME)
                .then(function (response) {
                console.log("Attendee exists REsponse : " + response.PersonExists);
                assert.equal(true, response.PersonExists);
                response.should.have.property('PersonId');
                done();
            })
                .catch(function (err) {
                done(err);
            });
        });
    });
});
//# sourceMappingURL=MeetupFaceManagerAttendeeExistsTest.js.map