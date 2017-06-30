import React from 'react';
import ReactDOM from 'react-dom';
import DropzoneComponent from 'react-dropzone-component';
import queryString from 'query-string';
// import queryString from  'query-string';

export default class RegisterAttendeeFace extends React.Component {

    constructor(props) {
        super(props);
    //    (new URLSearchParams(props.location.search)).get('eventName')
    //    const parsed = queryString.parse(props.location.search);
        // For a full list of possible configurations,
        // please consult http://www.dropzonejs.com/#configuration
        this.djsConfig = {
            acceptedFiles: "image/jpeg,image/png,image/gif",
            addRemoveLinks: false,
            params: {
                // eventName: 'Azure-Cloud_Meetup',
                eventName: (new queryString.parse(this.props.location.search)).event_name,
                // participantEmail: 'participant@outlook.com'
                participantEmail: (new queryString.parse(this.props.location.search)).participant_email
            }
        };

        this.componentConfig = {
            iconFiletypes: ['.jpg', '.png', '.gif'],
            showFiletypeIcon: true,
            postUrl: '/uploadHandler'
        };

        // If you want to attach multiple callbacks, simply
        // create an array filled with all your callbacks.
        this.callbackArray = [() => console.log('Callback received')];

        // Simple callbacks work too, of course
        this.callback = () => console.log('Hello!');
        // this.successCallback = (file, response) => alert("Face Registered.");
    }

    render() {
        const config = this.componentConfig;
        const djsConfig = this.djsConfig;
          
        // For a list of all possible events (there are many), see README.md!
        const eventHandlers = {
            drop: this.callbackArray,
            addedfile: this.callback,
           // success: this.successCallback,
        }

        return   <div> <h4>Meetup : {(new queryString.parse(this.props.location.search)).event_name} </h4> <h4>Attendee : {(new queryString.parse(this.props.location.search)).participant_email} </h4> <DropzoneComponent config={config} eventHandlers={eventHandlers} djsConfig={djsConfig}  /></div>;
    }
}
