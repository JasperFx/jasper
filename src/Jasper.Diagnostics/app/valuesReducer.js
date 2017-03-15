const initialState = {
  values: []
}

export default (state = initialState, action = {}) => {
  switch(action.type) {
    case 'SET_VALUES':
      return {
        values: action.values
      }
    default: return state;
  }
}
