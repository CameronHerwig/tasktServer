import React, { Component } from 'react';
import ReactDOM from 'react-dom';
import Button from '@material-ui/core/Button';
import ScriptsList from './components/ScriptsList';

export class Scripts extends Component {
    displayName = Scripts.name

    render() {
        return (
            <div>
                <h1>My Scripts</h1>     
                <p>View and manage scripts that your workers have uploaded for remote execution.</p>
                <ScriptsList api="/api/Scripts/All"></ScriptsList>
            </div>
        );
    }
}


