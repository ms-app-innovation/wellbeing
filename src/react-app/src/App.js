import React, { useState } from 'react';
import { Button } from './components/Button';
import { Textbox } from './components/Textbox';

function App() {   

  const [name, setName] = React.useState('');
  const [email, setEmail] = React.useState('');

  function callApi(score) { 
    fetch('/api/wellbeing', {
      method: 'post',
      headers: {'Content-Type':'application/json'},
      body: {
       "name": name,
       "email": email,
        "score": score 
      }
     }); 
  } 
  
  return (     
    <div> 
      <p>Hi there!</p>
      <p>Name: <Textbox text={name} onChange={e => setName(e.target.value)}/></p>
      <p>Email: <Textbox text={email} onChange={e => setEmail(e.target.value)}/></p>
      <p> How are you feeling today:? </p>
      <Button onClick={() => callApi(1)} text="1"/>   
      <Button onClick={() => callApi(2)} text="2"/>   
      <Button onClick={() => callApi(3)} text="3"/>   
      <Button onClick={() => callApi(4)} text="4"/>   
      <Button onClick={() => callApi(5)} text="5"/>         
    </div>     
  )   
} 

export default App;
