const express = require('express')
const bodyParser = require('body-parser');
const morgan = require('morgan');
const app = express()

app.set('port', process.env.PORT || 4000)
app.use(express.static(__dirname))
app.use(bodyParser.json());
app.use(morgan('common'));

app.get('/healthz', function (req, res) {
	// do app logic here to determine if app is truly healthy
	// you should return 200 if healthy, and anything else will fail
	// if you want, you should be able to restrict this to localhost (include ipv4 and ipv6)
  res.send('I am happy and healthy\n');
}); 

// Call the multerImpl and pass in app state to it
require('./src-server/ProcessAttendeeImage.js')(app)

module.exports = app.listen(app.get('port'), "0.0.0.0", () => {
  console.log('Express server listening on port ' + app.get('port'))
  console.log('Visit http://localhost:' + app.get('port') )
})

// quit on ctrl-c when running docker in terminal
process.on('SIGINT', function onSigint () {
	console.info('Got SIGINT (aka ctrl-c in docker). Graceful shutdown ', new Date().toISOString());
  shutdown();
});

// quit properly on docker stop
process.on('SIGTERM', function onSigterm () {
  console.info('Got SIGTERM (docker container stop). Graceful shutdown ', new Date().toISOString());
  shutdown();
})

// shut down server
function shutdown() {
  server.close(function onServerClosed (err) {
    if (err) {
      console.error(err);
      process.exitCode = 1;
		}
		process.exit();
  })
}