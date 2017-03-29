import { exec } from 'shelljs'
import { log, logError } from 'simple-make/lib/logUtils'
import open from 'open'
import { NPM_CMD, WEBPACK_DEV_PORT } from '../constants'

function wrapped(cmd) {
  exec(cmd, { async: true }, (code, stdout, stderr)=> {
    if(stdout) log(stdout)
    if(stderr) logError(stderr)
  })

  return Promise.resolve()
}

export default function run() {

  const webpackServer = wrapped(`${NPM_CMD} webpackServer`)
  const dotnetServer = wrapped(`${NPM_CMD} dotnetServer`)

  return Promise.all([webpackServer, dotnetServer]).then(()=> {
    open(`http://localhost:${WEBPACK_DEV_PORT}/_diag`)
  })
}
