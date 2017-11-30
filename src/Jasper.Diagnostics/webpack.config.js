import path from 'path'
import webpack from 'webpack'
import {
  createConfig,
  env,
  entryPoint,
  setOutput,
  sourceMaps,
  addPlugins
} from '@webpack-blocks/webpack'
import babel from '@webpack-blocks/babel6'
import devServer from '@webpack-blocks/dev-server'
import ExtractTextPlugin from 'extract-text-webpack-plugin'
import ManifestPlugin from 'webpack-manifest-plugin'
import {
  ENV,
  DEV_PORT,
  OUTPUT_PATH,
  JS_OUTPUT_FILENAME,
  CSS_OUTPUT_FILENAME,
  WEB_APP_ENTRY_POINT,
  WEB_APP_DIR,
  WEBPACK_DEV_PORT
} from './tools/constants'

const cssLoader = include => {
  return context => ({
    module: {
      loaders: [
        {
          test: /\.css$/,
          loader: ExtractTextPlugin.extract('style-loader', 'css-loader')
        }
      ]
    },
    plugins: [new ExtractTextPlugin(CSS_OUTPUT_FILENAME)]
  })
}

export default createConfig([
  entryPoint(path.join(__dirname, WEB_APP_ENTRY_POINT)),
  setOutput({
    path: path.join(__dirname, WEB_APP_DIR, OUTPUT_PATH),
    publicPath: '/_diag/',
    filename: JS_OUTPUT_FILENAME
  }),
  cssLoader(),
  addPlugins([
    // This helps ensure the builds are consistent if source hasn't changed:
    new webpack.optimize.OccurenceOrderPlugin(),
    new webpack.DefinePlugin({
      'process.env': {
        NODE_ENV: JSON.stringify(ENV)
      }
    }),
    new ManifestPlugin()
  ]),
  babel({
    exclude: /node_modules/
  }),
  env('development', [
    devServer([
      `webpack-dev-server/client?http://localhost:${WEBPACK_DEV_PORT}`,
      'webpack/hot/only-dev-server'
    ]),
    devServer.proxy({
      '*': { target: `http://localhost:${DEV_PORT}` }
    }),
    devServer.reactHot({
      exclude: /node_modules/
    }),
    sourceMaps()
  ]),
  env('production', [
    addPlugins([
      // Try to dedupe duplicated modules, if any:
      new webpack.optimize.DedupePlugin(),
      // Minify the code.
      new webpack.optimize.UglifyJsPlugin({
        compress: {
          screw_ie8: true, // React doesn't support IE8
          warnings: false
        },
        mangle: {
          screw_ie8: true
        },
        output: {
          comments: false,
          screw_ie8: true
        }
      })
    ])
  ])
])
