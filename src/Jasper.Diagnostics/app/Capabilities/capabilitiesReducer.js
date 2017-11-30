const initialState = {
  chains: [],
  publications: [],
  declaredSubscriptions: []
}

export default (state = initialState, action = {}) => {
  switch(action.type) {
    case 'diagnostic-data':
      return {
        chains: action.chains,
        publications: action.publications,
        declaredSubscriptions : action.declaredSubscriptions
      }
    default: return state
  }
}
