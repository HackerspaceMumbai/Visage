import React from 'react';
import ReactDOM from 'react-dom';
// import { browserHistory, Router, Route, Link, withRouter } from 'react-router';
import { BrowserRouter, Route } from 'react-router-dom'
import RegisterAttendeeFace from './RegisterAttendeeFace.jsx';

// Render

class RegisterAttendeeFaceMain extends React.Component {
    render() {
        return (
            <div>
                
                <div className="RegisterAttendeeFace.css">
                    <h4>Use with custom parameters</h4>
                    <p>This example simply showcases how one would use the component with custom POST parameters, for instance for authentication.</p>
                    <RegisterAttendeeFace />
                </div>
                
            </div>
        );
    }
}

// ReactDOM.render(<RegisterAttendeeFace />, document.getElementById('host'));
ReactDOM.render(
   (
     <BrowserRouter>
          <Route path="/" component={RegisterAttendeeFace}/>
     </BrowserRouter>
     )
    ,document.getElementById('host')
);
