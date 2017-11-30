import {
  default as React,
  PropTypes
} from 'react'
import { connect } from 'react-redux'
import Envelope from '../Components/Envelope'
import { toggleSavedMessage, RECV, SENT } from './messagesReducer'
import './RecentMessages.css'

const RecentMessages = ({ messages, saveMessage, direction }) => {
  const list = messages.map((m, idx) =>
    <li key={idx} className="message-list-item">
      <Envelope id={m.correlationId} direction={direction} saveMessage={saveMessage}/>
    </li>)
  return (
    <ul className="message-list">
      {list}
    </ul>
  )
}

RecentMessages.propTypes = {
  messages: PropTypes.array.isRequired,
  saveMessage: PropTypes.func.isRequired,
  direction: PropTypes.oneOf([SENT,RECV]).isRequired
}

export default connect(
  (state, props) => {
    return {
      messages: state.messages[props.direction + "Messages"].filter(m => m.messageType.fullName === props.typeName)
    }
  },
  (dispatch, props) => {
    return {
      saveMessage: (m) => {
        //i don't know if was coming or going so search both to set it as saved
        return dispatch(toggleSavedMessage(m, props.direction))
      }
    }
  }
)(RecentMessages)
