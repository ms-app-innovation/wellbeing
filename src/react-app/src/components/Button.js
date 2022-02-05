import React from 'react';

const Button = (props) => {
    return (
        <button className="AddButton" onClick={props.onClick}>{props.text}</button>
    );
}

export {Button};