import {
  default as React,
  PropTypes
} from 'react'
import { connect } from 'react-redux'
import AwesomeIcon from '../Components/AwesomeIcon'
import Button from '../Components/Button'
import ButtonGroup from '../Components/ButtonGroup'
import Envelope from '../Components/Envelope'
import Star from '../Components/Star'
import StatusIndicator from '../Components/StatusIndicator'
import {
  setMessageFilter,
  toggleSavedMessage,
  getVisibleMessages,
  getMessageFilter,
  RECV,
  SENT
} from './messagesReducer'

const checkIfSelected = (mode, selectedMode) => mode === selectedMode

const Messages = ({ filter, messages, onFilterClick, onSaveClick, direction }) => {
  let list = messages.map(m =>
    <li key={m.correlationId} className="message-list-item">
      <Envelope id={m.correlationId} direction={direction} saveMessage={onSaveClick} />
    </li>)
  return (
    <div>
      <ButtonGroup className="filter-group">
        <Button
          click={()=>onFilterClick('all')}
          selected={checkIfSelected('all', filter)}>
          <AwesomeIcon icon="circle-o"/> All
        </Button>
        <Button
          click={()=>onFilterClick('successful')}
          selected={checkIfSelected('successful', filter)}>
          <StatusIndicator success={true}/> Success
        </Button>
        <Button
          click={()=>onFilterClick('failed')}
          selected={checkIfSelected('failed', filter)}>
          <StatusIndicator success={false}/> Failed
        </Button>
        <Button
          click={()=>onFilterClick('saved')}
          selected={checkIfSelected('saved', filter)}>
          <Star selected={true}/> Saved
        </Button>
      </ButtonGroup>
      <ul className="message-list">
        {list}
      </ul>
    </div>
  )
}

Messages.propTypes = {
  filter: PropTypes.oneOf(['all', 'successful', 'failed', 'saved']).isRequired,
  messages: PropTypes.array.isRequired,
  onFilterClick: PropTypes.func.isRequired,
  onSaveClick: PropTypes.func.isRequired,
  direction: PropTypes.oneOf([SENT,  RECV])
}

const ExportedMessages = connect(
  (state, props) => {
    // const messages = props.direction === SENT ? state.messages.sentMessages : state.messages.recvMessages;
    return {
      filter: getMessageFilter(state.messages,  props.direction),
      messages: getVisibleMessages(state.messages, props.direction)
    }
  },
  (dispatch, props) => {
    return {
      onFilterClick: (filter) => {
        return dispatch(setMessageFilter(filter, props.direction))
      },
      onSaveClick: (message) => {
        return dispatch(toggleSavedMessage(message, props.direction))
      }
    }
  }
)(Messages)

ExportedMessages.propTypes = {
  direction: PropTypes.oneOf([SENT,  RECV])
}

export default ExportedMessages
