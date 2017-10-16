import {
  default as React,
  PropTypes
} from 'react'
import { connect } from 'react-redux'
import { push } from 'react-router-redux'
import Star from './Star'
import StatusIndicator from './StatusIndicator'

function Envelope({ id, queue, message, onClick, onNavigate }) {
  const star = ev => {
    ev.stopPropagation()
    onClick(message)
  }
  return (
    <div onClick={() => onNavigate(id, queue)} className="clickable">
      <StatusIndicator success={!message.hasError}/>
      <Star selected={message.saved === true} className="clickable" onClick={star}/>
      {message.description}
    </div>
  )
}

Envelope.propTypes = {
  id: PropTypes.string.isRequired,
  queue: PropTypes.string.isRequired,
  message: PropTypes.shape({
    description: PropTypes.string.isRequired,
    saved: PropTypes.bool.isRequired,
    hasError: PropTypes.bool.isRequired
  }),
  onClick: PropTypes.func,
  onNavigate: PropTypes.func.isRequired,
  saveMessage: PropTypes.func.isRequired
}

 const cEnv = connect(
  (state, props) => {
    return {
      message: state[props.queue].messages.find(m => m.correlationId === props.id)
    }
  },
  (dispatch, props) => {
    return {
      onClick: message => {
        return dispatch(props.saveMessage(message))
      },
      onNavigate: (id, queue) => {
        return dispatch(push(`/envelope/${queue}/${id}`))
      }
    }
  }
)(Envelope)

cEnv.propTypes = {
  id: PropTypes.string.isRequired,
  queue: PropTypes.string.isRequired,
  saveMessage: PropTypes.func.isRequired
}

export default cEnv
