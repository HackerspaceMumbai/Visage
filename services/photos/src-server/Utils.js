module.exports = {

  GetEventIdUrlParamName: function(){
  if(this.ValueExists(process.env.EVENT_ID_VARIABLE_NAME))
    return process.env.EVENT_ID_VARIABLE_NAME;
  else
    return "event_name"; // default value
  },

  GetParticipantIdUrlParamName: function(){
  if(this.ValueExists(process.env.PARTICIPANT_ID_VARIABLE_NAME))
    return process.env.PARTICIPANT_ID_VARIABLE_NAME;
  else
    return "participant_email"; // default value
  },

 ValueExists: function (value){
  if(value != undefined && value != '' )
    return true;
  else
    return false;
},

GetProfileIdFromProfileCookie: function(profileCookie){
  return "tempValue";
}



}