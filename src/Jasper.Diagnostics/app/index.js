import React from 'react'
import { render } from 'react-dom'
import { HashRouter } from 'react-router'

import App from './App'

const settings = window.DiagnosticsSettings;

render(
  <HashRouter>
    <App settings={settings}/>
  </HashRouter>,
  document.getElementById('root')
)
