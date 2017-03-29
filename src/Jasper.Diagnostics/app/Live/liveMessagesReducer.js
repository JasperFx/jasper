import moment from 'moment'
import orderBy from 'lodash/orderBy'

const initialState = {
  messages: [],
  savedMessages: [],
  filter: 'all'
}

const toggleSavedMessage = (message) => {
  return {
    type: 'toggle-saved-message',
    message
  }
}

const setMessageFilter = (value) => {
  return {
    type: 'set-message-filter',
    value
  }
}

const getVisibleMessages = (state, filter) => {
  switch(filter) {
    case 'successful':
      return orderBy(state.messages.filter(m => m.hasError === false), 'timestamp', 'desc')
    case 'failed':
      return orderBy(state.messages.filter(m => m.hasError === true), 'timestamp', 'desc')
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

const insert = (array, index, ...items) => {
  return [
    ...array.slice(0, index),
    ...items,
    ...array.slice(index)
  ]
}

export default (state = initialState, action = {}) => {
  switch(action.type) {
    case 'bus-message-succeeded':
      action.envelope.saved = false
      action.envelope.timestamp = moment().format('x')
      return {
        ...state,
        messages: insert(state.messages, 0, action.envelope)
      }
    case 'bus-message-failed':
      action.envelope.saved = false
      action.envelope.timestamp = moment().format('x')
      return {
        ...state,
        messages: insert(state.messages, 0, action.envelope)
      }
    case 'set-message-filter':
      return {
        ...state,
        filter: action.value
      }
    case 'toggle-saved-message': {
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
