import make from 'simple-make/lib/make';
import config from 'simple-make/lib/config';

import compile from './tasks/compile';
import restore from './tasks/restore';
import run from './tasks/run';

import webpackBuild from './tasks/webpackBuild';

config.name = '[diagnostics]';

const tasks = {
  'default': 'run',
  build: webpackBuild,
  compile: ['restore', compile],
  restore,
  run
};

make({ tasks });
