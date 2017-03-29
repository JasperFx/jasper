import moment from 'moment'
import sortBy from 'lodash/sortBy'

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
      return sortBy(state.messages.filter(m => m.hasError === false), 'timestamp')
    case 'failed':
      return sortBy(state.messages.filter(m => m.hasError === true), 'timestamp')
    case 'saved':
      return sortBy(state.savedMessages, 'timestamp')
    default:
      return sortBy(state.messages, 'timestamp')
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
      action.envelope.timestamp = moment().format()
      return Object.assign({}, state, {
        messages: insert(state.messages, 0, action.envelope)
      })
    case 'bus-message-failed':
      action.envelope.saved = false
      action.envelope.timestamp = moment().format()
      return Object.assign({}, state, {
        messages: insert(state.messages, 0, action.envelope)
      })
    case 'set-message-filter':
      return Object.assign({}, state, {
        filter: action.value
      })
    case 'toggle-saved-message': {
      const wasSaved = action.message.saved
      action.message.saved = !action.message.saved

      if (!wasSaved) {
        return Object.assign({}, state, {
          messages: state.messages.filter(m => m.correlationId !== action.message.correlationId).concat([action.message]),
          savedMessages: insert(state.savedMessages, 0, action.message)
        })
      } else {
        return Object.assign({}, state, {
          messages: state.messages.filter(m => m.correlationId !== action.message.correlationId).concat([action.message]),
          savedMessages: state.savedMessages.filter(m => m.headers.id !== action.message.headers.id)
        })
      }
    }
    default: return state
  }
}
