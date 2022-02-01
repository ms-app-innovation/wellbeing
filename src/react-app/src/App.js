import React from 'react';
import { Button } from './components/Button';
import { useState } from 'react'; 

function App() { 
  
  const [components, setComponents] = useState(["Sample Component"]); 
  
  function callApi(score) { 
    alert(score);    
  } 
  
  return (     
    <div> 
      <p>Hi there!</p>
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
