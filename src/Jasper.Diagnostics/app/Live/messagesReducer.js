import moment from 'moment'
import orderBy from 'lodash/orderBy'

const SENT = "sent"
const RECV = "recv"
const MAX_MESSAGES = 10000

const initialState = {
  sentMessages: [],
  recvMessages: [],
  sentFilter: 'all',
  recvFilter: 'all'
}

const toggleSavedMessage = (message, direction) => {
  return {
    type: 'toggle-saved-message',
    message,
    direction
  }
}

const setMessageFilter = (value, direction) => {
  return {
    type: 'set-message-filter',
    value,
    direction
  }
}

const getVisibleMessages = (state, direction) => {
  const messages = state[direction + "Messages"]
  const filter = getMessageFilter(state,  direction);
  switch(filter) {
    case 'successful':
      return orderBy(messages.filter(m => m.hasError === false), 'timestamp', 'desc').slice(0, MAX_MESSAGES)
    case 'failed':
      return orderBy(messages.filter(m => m.hasError === true), 'timestamp', 'desc').slice(0, MAX_MESSAGES)
    case 'saved':
      return orderBy(messages.filter(m => m.saved === true), 'timestamp', 'desc').slice(0, MAX_MESSAGES)
    default:
      return orderBy(messages, 'timestamp', 'desc').slice(0, MAX_MESSAGES)
  }
}

const getMessageFilter = (state, direction) =>{
  switch (direction){
    case SENT:
      return state.sentFilter
    case RECV:
      return state.recvFilter
    default:
      console.log('unknown direction')
      return 'all';
  }
}

export {
  getVisibleMessages,
  setMessageFilter,
  toggleSavedMessage,
  getMessageFilter,
  SENT,
  RECV
}

export default (state = initialState, action = {}) => {
  switch(action.type) {
    case 'bus-message-sent':
      action.envelope.saved = false
      action.envelope.timestamp = moment().format('x')
      return {
        ...state,
        sentMessages: [
          action.envelope,
          ...state.sentMessages
        ]
      }
    case 'bus-message-succeeded':
      action.envelope.saved = false
      action.envelope.timestamp = moment().format('x')
      return {
        ...state,
        recvMessages: [
          action.envelope,
          ...state.recvMessages
        ]
      }
    case 'bus-message-failed':
      action.envelope.saved = false
      action.envelope.timestamp = moment().format('x')
      return {
        ...state,
        recvMessages: [
          action.envelope,
          ...state.recvMessages
        ]
      }
    case 'set-message-filter':
      return {
        ...state,
        recvFilter: action.direction === RECV ? action.value : state.recvFilter,
        sentFilter: action.direction === SENT ? action.value : state.sentFilter
      }
    case 'toggle-saved-message': {
      const save = !action.message.saved
        return {
          ...state,
          recvMessages: state.recvMessages.map(m => action.direction === RECV && m.correlationId === action.message.correlationId ?
            { ...m, saved: save} : m),
          sentMessages: state.sentMessages.map(m => action.direction === SENT &&  m.correlationId === action.message.correlationId ?
            { ...m, saved: save} : m),
        }
    }
    default: return state
  }
}
