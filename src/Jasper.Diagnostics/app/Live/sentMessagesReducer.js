import moment from 'moment'
import orderBy from 'lodash/orderBy'

const initialState = {
  messages: [],
  savedMessages: [],
  filter: 'all'
}

const toggleSavedMessage = (message) => {
  return {
    type: 'toggle-sent-saved-message',
    message
  }
}

const setMessageFilter = (value) => {
  return {
    type: 'set-sent-message-filter',
    value
  }
}

const getVisibleMessages = (state, filter) => {
  switch(filter) {
    case 'saved':
      return orderBy(state.savedMessages, 'timestamp', 'desc')
    default:
      return orderBy(state.messages, 'timestamp', 'desc')
  }
}

export {
  getVisibleMessages,
  setMessageFilter,
  toggleSavedMessage
}

export default (state = initialState, action = {}) => {
  switch(action.type) {
    case 'bus-message-sent':
      action.envelope.saved = false
      action.envelope.timestamp = moment().format('x')
      return {
        ...state,
        messages: [
          action.envelope,
          ...state.messages
        ]
      }
    case 'set-sent-message-filter':
      return {
        ...state,
        filter: action.value
      }
    case 'toggle-sent-saved-message': {
      const save = !action.message.saved

      if (save === true) {
        return {
          ...state,
          messages: state.messages.map(m => m.correlationId == action.message.correlationId ?
            { ...m, saved: save} :
            m
          ),
          savedMessages: [
            ...state.savedMessages.filter(m => m.correlationId !== action.message.correlationId),
            { ...action.message, saved: save }
          ]
        }
      } else {
        return {
          ...state,
          messages: state.messages.map(m => m.correlationId == action.message.correlationId ?
            { ...m, saved: save} :
            m
          ),
          savedMessages: [
            ...state.savedMessages.filter(m => m.correlationId !== action.message.correlationId)
          ]
        }
      }
    }
    default: return state
  }
}
