module.exports = (app) => {
    const multer = require('multer');
    require('dotenv-extended').load();
    //const bodyParser = require('body-parser');
    var oxford = require('project-oxford'),
    client = new oxford.Client(process.env.FACE_API_KEY);

   

    const upload = multer();
        

    app.post('/uploadHandler', function (req, res, next) {
        if (req.file && req.file.originalname) {
            console.log(`Received file ${req.file.originalname}`);

        }

        if (req.body) {

            var bodyJSONString = JSON.stringify(req.body);
            var bodyJSON = JSON.parse(bodyJSONString);
            console.log(bodyJSON);
            console.log(bodyJSON.eventName);
            // console
            client.face.personGroup.get(bodyJSON.eventName)
            .then(function (response){
                console.log(response);
                console.log(JSON.parse(JSON.stringify(response)).personGroupId);
            }).catch(function(err)
            {
                console.log(err);
                console.log("No Such Meetup group");
            });


        }



        res.send({ responseText: req.file.path }); // You can send any response to the user here
    });

    app.get('/',function(req, res){
        res.sendFile('index.html');
    });
}