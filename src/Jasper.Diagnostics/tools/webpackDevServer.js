import webpack from 'webpack'
import WebpackDevServer from 'webpack-dev-server'
import { log, logError } from 'simple-make/lib/logUtils'

import config from '../webpack.config'
import { DEV_PORT, WEBPACK_DEV_PORT } from './constants'

new WebpackDevServer(webpack(config), {
  publicPath: config.output.publicPath,
  hot: true,
  historyApiFallback: true,
  proxy: {
    '*': `http://localhost:${DEV_PORT}`
  },
  stats: 'errors-only'
}).listen(WEBPACK_DEV_PORT, 'localhost', function(err, result) {
  if(err) {
    return logError('[webpack dev server]', err)
  }

  log('[webpack dev server]', `listening at http://localhost:${WEBPACK_DEV_PORT}`)
})
