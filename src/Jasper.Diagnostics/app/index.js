import React from 'react'
import { render } from 'react-dom'
import { createStore, combineReducers, applyMiddleware } from 'redux'
import thunk from 'redux-thunk'
import { Provider } from 'react-redux'
import { ConnectedRouter, routerReducer, routerMiddleware } from 'react-router-redux'
import createHistory from 'history/createHashHistory'
import { composeWithDevTools } from 'redux-devtools-extension'

const history = createHistory()
const middleware = routerMiddleware(history)

import Communicator from './communicator'
import rootReducer from './rootReducer'
import App from './App'

const store = createStore(
  combineReducers({
    ...rootReducer,
    router: routerReducer
  }),
  composeWithDevTools(
    applyMiddleware(thunk, middleware)
  )
)

const settings = window.DiagnosticsSettings

const communicator = new Communicator(store.dispatch, settings.websocketAddress, ()=> {
  store.dispatch({ type: 'disconnected' })
})

render(
  <Provider store={store}>
    <ConnectedRouter history={history}>
      <App settings={settings} communicator={communicator}/>
    </ConnectedRouter>
  </Provider>,
  document.getElementById('root')
)
