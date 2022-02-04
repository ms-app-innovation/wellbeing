import React, { useState, useEffect } from 'react';
import { Button } from './components/Button';
// import { Textbox } from './components/Textbox';

function App() {   

  // const [name, setName] = React.useState('');
  // const [email, setEmail] = React.useState('');
  const [userInfo, setUserInfo] = useState();

  useEffect(() => {
    (async () => {
      setUserInfo(await getUserInfo());
    })();
  }, []);


  function callApi(score) { 
    fetch('/api/wellbeing', {
      method: 'post',
      headers: {'Content-Type':'application/json'},      
      body: JSON.stringify({
       "name": userInfo.userDetails,
        "email": userInfo.userDetails,
        "score": score 
      })
     }).then(response => response.json().then(data => {alert(data)})); 
  } 
 
  async function getUserInfo() {
    try {
      const response = await fetch('/.auth/me');
      const payload = await response.json();
      const { clientPrincipal } = payload;
      return clientPrincipal;
    } catch (error) {
      console.error('No profile could be found');
      return undefined;
    }
  }

  
  return (     
    <div> 
      <h2>Contoso Wellbeing App</h2>
      <p>Hi {userInfo && userInfo.userDetails},</p>
      <p>{!userInfo && <a href="/.auth/login/aad">Click here to Login</a>}</p>
      <p>{userInfo && <a href={"/.auth/logout"}>Logout</a>}</p>
      {/* <p>Name: <Textbox text={name} onChange={e => setName(e.target.value)}/></p>
      <p>Email: <Textbox text={email} onChange={e => setEmail(e.target.value)}/></p> */}
      
        {userInfo && <div>
          <p> How are you feeling today:? </p>
        <Button onClick={() => callApi(1)} text="1"/>   
        <Button onClick={() => callApi(2)} text="2"/>   
        <Button onClick={() => callApi(3)} text="3"/>   
        <Button onClick={() => callApi(4)} text="4"/>   
        <Button onClick={() => callApi(5)} text="5"/>      
        </div>
        }
      
    </div>     
  )   
} 

export default App;
