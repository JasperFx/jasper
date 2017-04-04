import app from './appReducer'
import handlerChains from './HandlerChains/handlerChainsReducer'
import live from './Live/liveMessagesReducer'
import sent from './Live/sentMessagesReducer'

export default {
  app,
  handlerChains,
  live,
  sent
}
