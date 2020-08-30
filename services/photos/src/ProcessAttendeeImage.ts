import {Request, Response, NextFunction, Application} from 'express';
import ''

require('dotenv-extended').load();
module.exports = (app: Express.Application) => {
    
    const multer = require('multer');

    var meetupFaceManager = require('./MeetupFaceManager.js');
    //const bodyParser = require('body-parser');


    const storage = multer.memoryStorage();  

    const upload = multer({ storage: storage });

    app.post('/uploadHandler', upload.single('file'), function (req: Express.Request , res: Response, next: NextFunction) {
        if (req.file && req.file.originalname) {
            console.log(`Received file ${req.file.originalname}`);
            console.log(req.file);

        }

        var responseArgs = {};

        if (req.filebody) {

            var bodyJSONString = JSON.stringify(req.body);
            var bodyJSON = JSON.parse(bodyJSONString);
            var eventName = bodyJSON.eventName;
            var attendeeEmail = bodyJSON.participantEmail;
            // console.log(bodyJSON.eventName);
            console.log(eventName);
            var attendeeExists = false;
            var eventExists = false;
            var personId = null;
            // meetupFaceManager.EventExists()
            meetupFaceManager.EventExists(eventName)
                .then(function (response) {
                    console.log(response);
                    eventExists = response.EventExists;
                    if (eventExists) {
                        meetupFaceManager.AttendeeExists(eventName, attendeeEmail)
                            .then(function (response) {
                                console.log(response);
                                attendeeExists = response.PersonExists;
                                if (attendeeExists) {
                                    personId = response.PersonId;
                                    meetupFaceManager.AddPersonFace(eventName, personId, req.file.buffer)
                                        .then(function (response) {
                                            console.log(response);
                                            console.log('Face Added');
                                            res.send({ responseText: 'Face Successfully Added' });
                                        })

                                }
                                else {
                                    meetupFaceManager.AddPerson(eventName, attendeeEmail)
                                        .then(function (response) {
                                            console.log(response);
                                            if (response.PersonId != null) {
                                                personId = response.PersonId;
                                                meetupFaceManager.AddPersonFace(eventName, personId, req.file.buffer)
                                                    .then(function (response) {
                                                        console.log(response);
                                                        console.log('Face Added');
                                                        res.send({ responseText: 'Face Successfully Added' });
                                                    })
                                            }
                                            else {
                                                res.status(500).send('Could Not Create Add user to the event. Please verify details and try again.');

                                            }
                                        })

                                }

                            })

                    }
                    else {
                        res.status(500).send('Event name mentioned does not exist. Please verify');
                    }
                }

                )


                ;



            

        }

    });


}