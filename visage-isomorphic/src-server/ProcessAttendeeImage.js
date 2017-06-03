module.exports = (app) => {
    const multer = require('multer');
    //const bodyParser = require('body-parser');

    const storage = multer.diskStorage({
        destination: './uploads/',
        filename: function (req, file, cb) {
            // Mimetype stores the file type, set extensions according to filetype
            switch (file.mimetype) {
                case 'image/jpeg':
                    ext = '.jpeg';
                    break;
                case 'image/png':
                    ext = '.png';
                    break;
                case 'image/gif':
                    ext = '.gif';
                    break;
            }

            cb(null, file.originalname.slice(0, 4) + Date.now() + ext);
        }
    });
    const upload = multer({ storage: storage });
    //app.use(bodyParser.json());

    app.post('/uploadHandler', upload.single('file'), function (req, res, next) {
        if (req.file && req.file.originalname) {
            console.log(`Received file ${req.file.originalname}`);


        }

        if (req.body) {

            var bodyJSONString = JSON.stringify(req.body);
            var bodyJSON = JSON.parse(bodyJSONString);
            console.log(bodyJSON);
            console.log(bodyJSON.eventName);



        }



        res.send({ responseText: req.file.path }); // You can send any response to the user here
    });
}
