import webpack from 'webpack'
import ProgressPlugin from 'webpack/lib/ProgressPlugin'
import fs from 'fs'
import path from 'path'
import { log } from 'simple-make/lib/logUtils'
import Deferred from 'simple-make/lib/Deferred'
import { rm } from 'shelljs'

import { OUTPUT_PATH, WEB_APP_DIR, WEB_APP_ENV } from '../constants'

import webpackConfig from '../../webpack.config'

export default function webpackBuild() {
  const deferred = new Deferred()

  log(`NODE_ENV=${process.env.NODE_ENV}`)
  log(`ASPNETCORE_ENVIRONMENT=${WEB_APP_ENV}`)

  log(`Cleaning output directory: ${OUTPUT_PATH}`)
  rm('-rf', OUTPUT_PATH)

  if(process.env.NODE_ENV === undefined) {
    deferred.reject('NODE_ENV is undefined')
    return deferred.promise
  }

  bundle(webpackConfig, deferred)

  return deferred.promise
}

function bundle(config, deferred) {
  const compiler = webpack(config)

  compiler.apply(new ProgressPlugin((percentage, msg) => {
    if (!msg.match(/build modules/)) {
      log('[webpack]', msg)
    }
  }))

  compiler.run((err, rawStats) => {
    if(err) {
      deferred.reject(err)
      return
    }

    const stats = rawStats.toJson()

    if (stats.errors.length) {
      deferred.reject(stats.errors[0])
      return
    }

    const statsPath = path.join(WEB_APP_DIR, 'stats.json')
    fs.writeFileSync(statsPath, JSON.stringify(stats, null, 2))
    log('wrote file', statsPath)
    deferred.resolve()
  })
}
