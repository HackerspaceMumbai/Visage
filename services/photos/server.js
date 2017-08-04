const express = require('express')
const path = require('path');
const authenticationManager = require('./src-server/AuthenticationManager.js');
const util = require('./src-server/Utils.js');
const bodyParser = require('body-parser');
const morgan = require('morgan');
const app = express()
const cookieParser = require('cookie-parser');

app.set('port', process.env.PORT || 4000)
app.use(express.static(__dirname))
app.use(bodyParser.json());
app.use(cookieParser());
app.use(morgan('common'));

app.get('/healthz', function (req, res) {
  // do app logic here to determine if app is truly healthy
  // you should return 200 if healthy, and anything else will fail
  // if you want, you should be able to restrict this to localhost (include ipv4 and ipv6)
  res.send('I am happy and healthy\n');
});


app.get('/', function (req, res) {

  var profileCookie = req.cookies.profile;
  var externalAuthentication =  authenticationManager.GetParticipantProfileByOAuth();
  var eventIdParameterName = util.GetEventIdUrlParamName();
  var participantIdParameterName = util.GetParticipantIdUrlParamName();
  var eventIdParameterValue = util.ValueExists(req.query[eventIdParameterName]) ? req.query[eventIdParameterName] : undefined;
  var participantIdParameterValue = util.ValueExists(req.query[participantIdParameterName]) ? req.query[eventIdParameterName] : undefined ;  

  var response = authenticationManager.GetNextAction(profileCookie,externalAuthentication,eventIdParameterValue, participantIdParameterValue);

  switch(response.actionType){
    case util.ActionType.REDIRECT_OAUTH: {
      // If Event Name exists and eventbrite OAuth configured, and profile cookie not present
      console.log("Initiating OAuth exchange with eventbrite.. redirecting to login_with service");
      res.redirect(response.redirectUrl);
      break;
    }
    case util.ActionType.RENDER_DEFAULT_FILE:{
      // If Eventbrite authentication not configured and valid url parameters provided, or Eventbrite authentication configured and valid profile token found and parameters provided
      console.log("Fetching Meetup Id and Participant Id from querysting");
      res.sendFile(path.resolve(__dirname + '/public/index.html'));
      break;
    }
    case util.ActionType.REDIRECT_SUCCESS: {
      // If Event Name exists in param and Partcipant Id param does not exist, redirect to success route. This would happen if OAuth exchange happens but for some reason user has removed participant id from URL
      console.log("Redirect to Success to fetch participant id from cookie");
      res.redirect(response.redirectUrl);
      break;
    }
    case util.ActionType.SHOW_BAD_REQUEST_MESSAGE: {
      // If invalid parameters provided, return bad request
      console.log("Invalid Request.. Either incorrect query string parameters provided in request, or application not configured correctly");
      res.status(400).send("HTTP 400 : Bad Request : Please check the Link and try again");
      break;
  }
  }

  



});

// This will be called by the login_with service if OAuth exchange is successful
app.get('/success/:eventName', function (req, res) {
  
  console.log("Oauth Response Received..");

  if (req.cookies.profile) {
    console.log("Oauth Successful");
    // res.sendFile(path.resolve(__dirname + '/public/index.html'));
    res.redirect(authenticationManager.GetPostAuthenticationSuccessRedirectionUrl(req.cookies.profile, req.params.eventName));
  }
  else {
    console.log("Profile cookie not found after OAuth Exchange...");
    res.redirect(authenticationManager.GetPostAuthenticationFailureRedirectionUrl());
  }

});

// This will be called by the login_with service is OAuth exchange is unsuccessful
app.get('/failure', function (req, res) {
  console.log("Oauth Failure..");
  res.redirect(authenticationManager.GetPostAuthenticationFailureRedirectionUrl());
});


// Call the multerImpl and pass in app state to it
require('./src-server/ProcessAttendeeImage.js')(app)

module.exports = app.listen(app.get('port'), "0.0.0.0", () => {
  console.log('Express server listening on port ' + app.get('port'))
  console.log('Visit http://localhost:' + app.get('port'))
})

// quit on ctrl-c when running docker in terminal
process.on('SIGINT', function onSigint() {
  console.info('Got SIGINT (aka ctrl-c in docker). Graceful shutdown ', new Date().toISOString());
  shutdown();
});

// quit properly on docker stop
process.on('SIGTERM', function onSigterm() {
  console.info('Got SIGTERM (docker container stop). Graceful shutdown ', new Date().toISOString());
  shutdown();
})

// shut down server
function shutdown() {
  app.close(function onServerClosed(err) {
    if (err) {
      console.error(err);
      process.exitCode = 1;
    }
    process.exit();
  })
}

