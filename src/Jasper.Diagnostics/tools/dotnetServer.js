import { exec, pushd, popd } from 'shelljs'
import { log, logError } from 'simple-make/lib/logUtils'

// dotnet watch appears to only work when it is launched from the
// directory of the application
// https://github.com/aspnet/DotNetTools/issues/147#issuecomment-257931808

const app = process.env.TARGET_APP

pushd(app)

const cmd = 'dotnet watch run'

log(cmd)

exec(cmd, { async: true }, (code, stdout, stderr)=> {
  if(stdout) log(stdout)
  if(stderr) logError(stderr)
})

popd()
