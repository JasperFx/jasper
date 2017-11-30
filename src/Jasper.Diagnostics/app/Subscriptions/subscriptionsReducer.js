const initialState = {
  subscriptions: []
}

export default (state = initialState, action = {}) => {
  switch(action.type) {
    case 'diagnostic-data':
      return {
        subscriptions: action.storedSubscriptions
      }
    default: return state
  }
}
