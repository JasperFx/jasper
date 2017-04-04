import {
  default as React,
  PropTypes
} from 'react'
import { connect } from 'react-redux'
import Envelope from '../Components/Envelope'
import {
  toggleSavedMessage
} from './liveMessagesReducer'
import './RecentMessages.css'

const RecentMessages = ({ messages, saveMessage }) => {
  const list = messages.map((m, idx) =>
    <li key={idx} className="message-list-item">
      <Envelope id={m.correlationId} queue="live" saveMessage={saveMessage}/>
    </li>)
  return (
    <ul className="message-list">
      {list}
    </ul>
  )
}

RecentMessages.propTypes = {
  messages: PropTypes.array.isRequired,
  saveMessage: PropTypes.func.isRequired
}

export default connect(
  (state, props) => {
    return {
      messages: state.live.messages.filter(m => m.messageType.fullName === props.typeName)
    }
  },
  (dispatch) => {
    return {
      saveMessage: (m) => {
        return dispatch(toggleSavedMessage(m))
      }
    }
  }
)(RecentMessages)
