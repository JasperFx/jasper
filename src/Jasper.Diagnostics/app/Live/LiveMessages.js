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
  getVisibleMessages
} from './liveMessagesReducer'

function checkIfSelected(mode, selectedMode) {
  return mode === selectedMode
}

const LiveMessages = ({ filter, messages, onFilterClick, onSaveClick }) => {
  let list = messages.map(m =>
    <li key={m.correlationId} className="message-list-item">
      <Envelope id={m.correlationId} queue="live" saveMessage={onSaveClick} />
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

LiveMessages.propTypes = {
  filter: PropTypes.oneOf(['all', 'successful', 'failed', 'saved']),
  messages: PropTypes.array.isRequired,
  onFilterClick: PropTypes.func.isRequired,
  onSaveClick: PropTypes.func.isRequired
}

export default connect(
  (state) => {
    return {
      filter: state.live.filter,
      messages: getVisibleMessages(state.live, state.live.filter)
    }
  },
  (dispatch) => {
    return {
      onFilterClick: (filter) => {
        return dispatch(setMessageFilter(filter))
      },
      onSaveClick: (message) => {
        return dispatch(toggleSavedMessage(message))
      }
    }
  }
)(LiveMessages)
