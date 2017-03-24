const initialState = {
  mode: 'connected'
}

export default (state = initialState, action = {}) => {
  switch(action.type) {
    case 'disconnected':
      return {
        mode: 'disconnected'
      }
    default: return state
  }
}
