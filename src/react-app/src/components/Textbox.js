import React from 'react'; 

const Textbox = (props) => { 
    return ( 
    <input type="text" onChange ={props.onChange} value= {props.text} />
  );   
} 

export {Textbox};