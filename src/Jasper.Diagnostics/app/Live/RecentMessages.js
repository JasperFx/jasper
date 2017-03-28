import {
  default as React,
  PropTypes
} from 'react'
import { connect } from 'react-redux'
import Envelope from '../Components/Envelope'
import './RecentMessages.css'

function RecentMessages({messages}) {
  const list = messages.map((m, idx) => <li key={idx} className="message-list-item"><Envelope id={m.correlationId}/></li>)
  return (
    <ul className="message-list">
      {list}
    </ul>
  )
}

RecentMessages.propTypes = {
  messages: PropTypes.array.isRequired
}

export default connect(
  (state, props) => {
    return {
      messages: state.live.messages.filter(m => m.messageType.fullName === props.typeName)
    }
  }
)(RecentMessages)
