import React from 'react'
import { render } from 'react-dom'
import { HashRouter } from 'react-router'
import { createStore, applyMiddleware } from 'redux';
import thunk from 'redux-thunk';
import { Provider } from 'react-redux';
import { composeWithDevTools } from 'redux-devtools-extension';

import Communicator from './communicator'
import rootReducer from './rootReducer';
import App from './App'

const store = createStore(
  rootReducer,
  composeWithDevTools(
    applyMiddleware(thunk)
  )
)

const settings = window.DiagnosticsSettings;

const communicator = new Communicator(() => {}, settings.websocketAddress, ()=> {})

render(
  <HashRouter>
    <Provider store={store}>
      <App settings={settings}/>
    </Provider>
  </HashRouter>,
  document.getElementById('root')
)
