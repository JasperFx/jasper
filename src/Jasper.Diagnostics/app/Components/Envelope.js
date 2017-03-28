import {
  default as React,
  PropTypes
} from 'react'
import { connect } from 'react-redux'
import { push } from 'react-router-redux'
import Star from './Star'
import StatusIndicator from './StatusIndicator'
import {
  toggleSavedMessage
} from '../Live/liveMessagesReducer'

function Envelope({ id, message, onClick, onNavigate }) {
  const star = ev => {
    ev.stopPropagation()
    onClick(message)
  }
  return (
    <div onClick={() => onNavigate(id)} className="clickable"><StatusIndicator success={!message.hasError}/><Star selected={message.saved === true} className="clickable" onClick={star}/>{message.description}</div>
  )
}

Envelope.propTypes = {
  id: PropTypes.string.isRequired,
  message: PropTypes.shape({
    description: PropTypes.string.isRequired,
    saved: PropTypes.bool.isRequired,
    hasError: PropTypes.bool.isRequired
  }),
  onClick: PropTypes.func,
  onNavigate: PropTypes.func.isRequired
}

export default connect(
  (state, props) => {
    return {
      message: state.live.messages.find(m => m.headers.id === props.id)
    }
  },
  (dispatch) => {
    return {
      onClick: message => {
        dispatch(toggleSavedMessage(message))
      },
      onNavigate: id => {
        dispatch(push(`/envelope/${id}`))
      }
    }
  }
)(Envelope)
