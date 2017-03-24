const initialState = {
  messages: [],
  savedMessages: [],
  filter: 'all'
}

export function setMessageFilter(value) {
  return {
    type: 'set-message-filter',
    value
  }
}

export function getVisibleMessages(messages, filter) {
  switch(filter) {
    case 'successful':
      return messages.filter(m => m.hasError === false)
    case 'failed':
      return messages.filter(m => m.hasError)
    default:
      return messages
  }
}

function insert(array, index, ...items) {
  return [
    ...array.slice(0, index),
    ...items,
    ...array.slice(index)
  ]
}

export default (state = initialState, action = {}) => {
  switch(action.type) {
    case 'bus-message-succeeded':
      return Object.assign({}, state, {
        messages: insert(state.messages, 0, action.envelope)
      })
    case 'bus-message-failed':
      return Object.assign({}, state, {
        messages: insert(state.messages, 0, action.envelope)
      })
    case 'set-message-filter':
      return Object.assign({}, state, {
        filter: action.value
      })
    default: return state
  }
}
