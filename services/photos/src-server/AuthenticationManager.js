const util = require('./Utils.js');
module.exports = {

  // Returns true if our service is configured to Authenticate participant using eventbrite
GetParticipantProfileByOAuth: function() {
  if (util.ValueExists(process.env.LOGIN_WITH_SERVICE_URI) && util.ValueExists(process.env.APP_DOMAIN_NAME) ) {
    return true;
  }
  else {
    return false;
  }

},

// Returns OAuth url passed to the login_with service, this includes the success and failure callbacks
GetExternalAuthenticationUrl: function(eventName) {
  if (util.ValueExists(process.env.LOGIN_WITH_SERVICE_URI) && util.ValueExists(process.env.APP_DOMAIN_NAME) ) {
    return process.env.LOGIN_WITH_SERVICE_URI + "/eventbrite?success=" + process.env.APP_DOMAIN_NAME + "/success/"+eventName+"&failure= " + process.env.APP_DOMAIN_NAME + "/failure";
  }
  else return '';
},

GetPostAuthenticationSuccessRedirectionUrl: function(profileCookie, eventName) {

  if(util.ValueExists(process.env.APP_DOMAIN_NAME))
    {
      var participantId = util.GetProfileIdFromProfileCookie(profileCookie);
      var eventParamName = util.GetEventIdUrlParamName();
      var participantParamName = util.GetParticipantIdUrlParamName();
      return process.env.APP_DOMAIN_NAME + "/?" + eventParamName + "=" + eventName + "&" + participantParamName + "=" + participantId ;
    }
  else 
    return "";
  
},

GetPostAuthenticationFailureRedirectionUrl: function(profileCookie, eventName) {

    return process.env.APP_DOMAIN_NAME;
  
}




}


