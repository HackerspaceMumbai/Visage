module.exports = {

  // Returns true if our service is configured to Authenticate participant using eventbrite
GetParticipantProfileByOAuth: function() {
  if (process.env.LOGIN_WITH_SERVICE_URI != undefined && process.env.LOGIN_WITH_SERVICE_URI != '') {
    return true;
  }
  else {
    return false;
  }

},

// Returns OAuth url passed to the login_with service, this includes the success and failure callbacks
GetLoginWithOAuthUrl: function() {
  if (process.env.LOGIN_WITH_SERVICE_URI != undefined && process.env.LOGIN_WITH_SERVICE_URI != '' && process.env.APP_DOMAIN_NAME != undefined && process.env.APP_DOMAIN_NAME != '') {
    return process.env.LOGIN_WITH_SERVICE_URI + "/eventbrite?success=" + process.env.APP_DOMAIN_NAME + "/success&failure= " + process.env.APP_DOMAIN_NAME + "/failure";
  }
  else return '';
}

}
