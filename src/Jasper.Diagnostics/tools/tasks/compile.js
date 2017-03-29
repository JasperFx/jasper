import { rm } from 'shelljs'
import exec from './exec'
import { TARGET, WEB_APP_DIR } from '../constants'

export default exec(()=> {
  rm('-rf', './obj')
  rm('-rf', './bin')

  const build = `dotnet build ${WEB_APP_DIR} -c ${TARGET}`
  return build
})
