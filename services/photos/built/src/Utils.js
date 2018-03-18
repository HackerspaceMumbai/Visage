"use strict";
module.exports = {
    DEFAULT_EVENT_PARAMETER_NAME: "event_name",
    DEFAULT_PARTICIPANT_PARAMETER_NAME: "participant_email",
    GetEventIdUrlParamName: function () {
        if (this.ValueExists(process.env.EVENT_ID_VARIABLE_NAME))
            return process.env.EVENT_ID_VARIABLE_NAME;
        else
            return this.DEFAULT_EVENT_PARAMETER_NAME; // default value
    },
    GetParticipantIdUrlParamName: function () {
        if (this.ValueExists(process.env.PARTICIPANT_ID_VARIABLE_NAME))
            return process.env.PARTICIPANT_ID_VARIABLE_NAME;
        else
            return this.DEFAULT_PARTICIPANT_PARAMETER_NAME; // default value
    },
    ValueExists: function (value) {
        if (value != undefined && value != '')
            return true;
        else
            return false;
    },
    GetProfileIdFromProfileCookie: function (profileCookie) {
        return JSON.parse(encodeURI(profileCookie)).username;
    },
    ActionType: {
        REDIRECT_OAUTH: 0,
        RENDER_DEFAULT_FILE: 1,
        SHOW_BAD_REQUEST_MESSAGE: 2,
        REDIRECT_SUCCESS: 3
    }
};
//# sourceMappingURL=Utils.js.map