module.exports = (app) => {
    const multer = require('multer');
    require('dotenv-extended').load();
    //const bodyParser = require('body-parser');
    var oxford = require('project-oxford'),
        client = new oxford.Client(process.env.FACE_API_KEY);


    const storage = multer.memoryStorage();

    const upload = multer({ storage: storage });

    app.post('/uploadHandler', upload.single('file'), function (req, res, next) {
        if (req.file && req.file.originalname) {
            console.log(`Received file ${req.file.originalname}`);
            console.log(req.file);

        }

        var responseArgs ={};

        if (req.body) {

            var bodyJSONString = JSON.stringify(req.body);
            var bodyJSON = JSON.parse(bodyJSONString);
            var eventName = bodyJSON.eventName;
            var attendeeEmail = bodyJSON.participantEmail;
            // console.log(bodyJSON);
            // console.log(bodyJSON.eventName);
            console.log(eventName);
            // console

            // Check if Meetup Group Exists
            client.face.personGroup.get(eventName)
                .then(function (response) {
                    console.log(response);
                    console.log(JSON.parse(JSON.stringify(response)).personGroupId);
                    console.log("Person Group Exists");

                    var personId = '';
                    client.face.person.create(eventName, attendeeEmail, attendeeEmail)
                        .then(function (response) {
                            console.log(response);
                            console.log('person created');
                            personId = JSON.parse(JSON.stringify(response)).personId;
                            console.log(personId);

                            client.face.person.addFace(eventName, personId, { data: req.file.buffer })
                                .then(function (response) {
                                    console.log(response);
                                    // console.log(JSON.parse(JSON.stringify(response)).personGroupId);
                                    responseArgs = {"status" : "success",  "responseText" : "Face Added"};
                                }).catch(function (err) {
                                    responseArgs = {"status" : "failed"};
                                    console.log(err);
                                    console.log("Could Not add Face");
                                    responseArgs = {"status" : "failed",  "responseText" : "Could Not add Face"};
                                });

                        }).catch(function (err) {
                            console.log(err);
                            console.log("Could Not Create Person");
                            responseArgs = {"status" : "failed",  "responseText" : "Could Not Create Person"};

                        });


                }).catch(function (err) {
                    console.log(err);
                    console.log("No Such Meetup group");
                    responseArgs = {"status" : "failed", "responseText" : "No Such Meetup group"};

                });


            // Add face for Person


        }



        res.send(responseArgs); // You can send any response to the user here
    });

    app.get('/', function (req, res) {
        res.sendFile('index.html');
    });
}