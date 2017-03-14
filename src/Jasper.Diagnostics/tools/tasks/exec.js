import { exec } from 'shelljs';
import Deferred from 'simple-make/lib/Deferred';
import { log } from 'simple-make/lib/logUtils';

export default function execFn(options) {
  return () => {
    const deferred = new Deferred();

    let cmd = options || '';

    if(typeof(cmd) === 'function') {
      cmd = options();
    }

    if(cmd === '' || cmd === undefined) {
      deferred.reject('must provide a command');
      return deferred.promise;
    }

    log(cmd);

    exec(cmd, (code, stdout, stderr)=> {
      if(code === 0) {
        deferred.resolve();
      } else {
        deferred.reject(stderr);
      }
    });
    return deferred.promise;
  }
}
