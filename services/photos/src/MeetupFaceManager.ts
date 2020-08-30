import * as oxford from 'project-oxford';
import * as dotenv from 'dotenv-extended';
dotenv.load();
let client= new oxford.Client(process.env.FACE_API_KEY);


module.exports = {


    EventExists: function (eventName: string) {       
        var promise = new Promise(function (resolve, reject) {
            client.face.personGroup.get(eventName)
                .then(function (response) {
                    console.log(response);
                    if (JSON.parse(JSON.stringify(response)).personGroupId != null && JSON.parse(JSON.stringify(response)).personGroupId != '') {
                        console.log("Person Group Exists");
                        resolve({ EventExists: true });
                    }
                    else {
                        console.log("Person Group Does not Exists");
                        resolve({ EventExists: false });
                    }

                })
                .catch(function (err) {
                    console.log(err);
                    console.log("Person Group Does not Exists");
                    resolve({ EventExists: false });
                })

        });
        return promise;
    },

    AttendeeExists: function (eventName: string, attendeeEmail:string) {
        return client.face.person.list(eventName)

            .then(function (response) {
                console.log(eventName);
                console.log(attendeeEmail);

                var retVal = response.find(o => o.name === attendeeEmail);
                if (retVal != undefined) {
                    console.log("Person Already Exists");
                    return ({ PersonExists: true, PersonId: retVal.personId });
                }
                else {
                    return ({ PersonExists: false });
                }


            })

            .catch(function (err) {
                console.log(err);
                console.log("Person  Does not Exists");
                return({ PersonExists: false });
            });


    },

    AddPerson: function (eventName: string, attendeeEmail:string) {
        var promise = new Promise(function (resolve, reject) {
            client.face.person.create(eventName, [], attendeeEmail, attendeeEmail)
                .then(function (response) {
                    if (JSON.parse(JSON.stringify(response)).personId != null) {
                        resolve({ PersonId: JSON.parse(JSON.stringify(response)).personId });
                        console.log("New Person Created");
                    }
                })
                .catch(function (err: Error) {
                    console.log(err);
                    console.log("Could Not Create New Person");
                    resolve({ PersonId: null });
                })

        });
        return promise;
    }

    ,

    AddPersonFace: function (eventName: string, personId: string, faceImageData: string ) {
        var promise = new Promise(function (resolve, reject) {
            client.face.person.addFace(eventName, personId, faceImageData )
                .then(function (response) {
                    console.log(response);
                    if (JSON.parse(JSON.stringify(response)).persistedFaceId != null && JSON.parse(JSON.stringify(response)).persistedFaceId != undefined) {

                        resolve({ FaceId: JSON.parse(JSON.stringify(response)).persistedFaceId });
                        console.log("Person Face Added");
                    }
                }
                )
                .catch(function (err: Error) {
                    console.log(err);
                    console.log("Could Not add Face Against Person");
                    resolve({ FaceId: null });
                })

        });
        return promise;
    }


}

