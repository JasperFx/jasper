const initialState = {
  chains: []
}

export default (state = initialState, action = {}) => {
  switch(action.type) {
    case 'initial-data':
      return {
        chains: action.chains
      }
    default: return state
  }
}
