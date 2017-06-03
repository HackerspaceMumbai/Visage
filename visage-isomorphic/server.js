const express = require('express')
const bodyParser = require('body-parser');
const app = express()

app.set('port', process.env.PORT || 4000)
app.use(express.static(__dirname))
app.use(bodyParser.json());

// Call the multerImpl and pass in app state to it
require('./src-server/ProcessAttendeeImage.js')(app)

module.exports = app.listen(app.get('port'), "0.0.0.0", () => {
  console.log('Express server listening on port ' + app.get('port'))
  console.log('Visit http://localhost:' + app.get('port') )
})
