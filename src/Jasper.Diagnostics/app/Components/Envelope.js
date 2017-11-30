import {
  default as React,
  PropTypes
} from 'react'
import { connect } from 'react-redux'
import { push } from 'react-router-redux'
import Star from './Star'
import StatusIndicator from './StatusIndicator'
import {SENT, RECV} from "../Live/messagesReducer"

function Envelope({ id, direction, message, onClick, onNavigate }) {
  const star = ev => {
    ev.stopPropagation()
    onClick(message)
  }
  if (!message)
    return <div onClick={() => onNavigate(id, direction)} className="clickable">Message no longer available!</div>
  return (
    <div onClick={() => onNavigate(id, direction)} className="clickable">
      <StatusIndicator success={!message.hasError}/>
      <Star selected={message.saved === true} className="clickable" onClick={star}/>
      {message.description}
    </div>
  )
}

Envelope.propTypes = {
  id: PropTypes.string.isRequired,
  direction: PropTypes.oneOf([SENT, RECV]).isRequired,
  message: PropTypes.shape({
    description: PropTypes.string.isRequired,
    saved: PropTypes.bool.isRequired,
    hasError: PropTypes.bool.isRequired
  }).isRequired,
  onClick: PropTypes.func,
  onNavigate: PropTypes.func.isRequired,
  saveMessage: PropTypes.func.isRequired
}

 const cEnv = connect(
  (state, props) => {
    return {
      message: state.messages[props.direction + "Messages"].find(m => m.correlationId === props.id)
    }
  },
  (dispatch, props) => {
    return {
      onClick: message => {
        return dispatch(props.saveMessage(message, props.direction))
      },
      onNavigate: (id, direction) => {
        return dispatch(push(`/envelope/${direction}/${id}`))
      }
    }
  }
)(Envelope)

cEnv.propTypes = {
  id: PropTypes.string.isRequired,
  direction: PropTypes.oneOf([SENT,RECV]).isRequired,
  saveMessage: PropTypes.func.isRequired
}

export default cEnv
