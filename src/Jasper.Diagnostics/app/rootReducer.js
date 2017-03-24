import { combineReducers } from 'redux'

import app from './appReducer'
import handlerChains from './HandlerChains/handlerChainsReducer'
import live from './Live/liveMessagesReducer'

export default combineReducers({
  app,
  handlerChains,
  live
})
