import app from './appReducer'
import capabilities from './Capabilities/capabilitiesReducer'
import messages from './Live/messagesReducer'
// import sent from './Live/sentMessagesReducer'
import subscriptionInfo from './Subscriptions/subscriptionsReducer'

export default {
  app,
  capabilities,
  messages,
  subscriptionInfo
}
