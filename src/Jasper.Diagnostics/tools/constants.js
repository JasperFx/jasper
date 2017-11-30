import changeCase from 'change-case'

export const DEV_PORT = 5000
export const WEBPACK_DEV_PORT = 3000
export const OUTPUT_PATH = './resources/'
export const JS_OUTPUT_FILENAME = 'index.js'
export const CSS_OUTPUT_FILENAME = 'main.css'
export const WEB_APP_ENTRY_POINT = './app/index.js'
export const WEB_APP_DIR = './'
export const HARNESS_DIR = '../DiagnosticsHarness'
export const NPM_CMD = 'yarn'
export const ENV = process.env.NODE_ENV ? process.env.NODE_ENV : 'development'
export const TARGET = process.env.CONFIGURATION ? process.env.CONFIGURATION : 'Debug'

// ASP.NET Core expects environment names to be Pascal case
export const WEB_APP_ENV = changeCase.pascal(ENV)
