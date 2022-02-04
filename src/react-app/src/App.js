import React, { useState, useEffect } from 'react';
import { Button } from './components/Button';


function App() {   
  const [api, setApi] = React.useState('/api/wellbeing-v1');
  const [userInfo, setUserInfo] = useState();
  const redirect = window.location.pathname;

  useEffect(() => {
    (async () => {
      setUserInfo(await getUserInfo());
    })();
  }, []);


  function callApi(score) {    
    fetch(api, {
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
      <p>{!userInfo && <a href={`/.auth/login/aad?post_login_redirect_uri=${redirect}`}>Click here to Login</a>}</p>
      <p>{userInfo && <a href={`/.auth/logout?post_logout_redirect_uri=${redirect}`}>Logout</a>}</p>
      {/* <p>Name: <Textbox text={name} onChange={e => setName(e.target.value)}/></p>
      <p>Email: <Textbox text={email} onChange={e => setEmail(e.target.value)}/></p> */}
      
        {userInfo && 
          <div>
            <p> How are you feeling today (on a scale of 1 to 5 with 5 being your best)?</p>
            <Button onClick={() => callApi(1)} text="1"/>   
            <Button onClick={() => callApi(2)} text="2"/>   
            <Button onClick={() => callApi(3)} text="3"/>   
            <Button onClick={() => callApi(4)} text="4"/>   
            <Button onClick={() => callApi(5)} text="5"/>      
            <p>
               Select the backend pattern:
                <select name="backendAPI" id="api" onChange={e => setApi(e.target.value)}>
                  <option value="/api/wellbeing-v1">v1 - Tight Coupling</option>
                  <option value="/api/wellbeing-v2">v2 - Storage Queue</option>
                  <option value="/api/wellbeing-v3">v3 - Outbox Pattern</option>
                  <option value="/api/wellbeing-v4">v4 - Broadcast Events</option>
                  <option value="/api/wellbeing-v5">v5 - Process Manager</option>
                  <option value="/api/wellbeing-v6">v6 - Choreography</option>
                  <option value="/api/wellbeing-v7">v7 - Routing Slip</option>
                  <option value="/api/wellbeing-v8">v8 - Process Manager v2</option>
                  <option value="/api/wellbeing-v9">v9 - Event Sourcing</option>
                </select>
            </p>
          </div>
        }
      
    </div>     
  )   
} 

export default App;
