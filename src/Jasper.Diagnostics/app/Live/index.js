import {
  default as React,
  PropTypes
} from 'react'
import { connect } from 'react-redux'
import AwesomeIcon from '../Components/AwesomeIcon'
import Button from '../Components/Button'
import ButtonGroup from '../Components/ButtonGroup'
import Row from '../Components/Row'
import Col from '../Components/Col'
import Card from '../Components/Card'
import Star from '../Components/Star'
import StatusIndicator from '../Components/StatusIndicator'
import Envelope from '../Components/Envelope'
import {
  setMessageFilter,
  toggleSavedMessage,
  getVisibleMessages
} from './liveMessagesReducer'
import './index.css'


function checkIfSelected(mode, selectedMode) {
  return mode === selectedMode
}

const Live = (props) => {
  const messages = props.messages.map((m, idx) => {
    return (
      <li key={idx} className="message-list-item"><Envelope id={m.headers.id}/></li>
    )
  })
  return (
    <Row>
      <Col column={12}>
        <Card>
          <h2 className="header-title">Latest Messages</h2>
          <ButtonGroup className="filter-group">
            <Button
              click={()=>props.onFilterClick('all')}
              selected={checkIfSelected('all', props.filter)}>
              <AwesomeIcon icon="circle-o"/> All
            </Button>
            <Button
              click={()=>props.onFilterClick('successful')}
              selected={checkIfSelected('successful', props.filter)}>
              <StatusIndicator success={true}/> Success
            </Button>
            <Button
              click={()=>props.onFilterClick('failed')}
              selected={checkIfSelected('failed', props.filter)}>
              <StatusIndicator success={false}/> Failed
            </Button>
            <Button
              click={()=>props.onFilterClick('saved')}
              selected={checkIfSelected('saved', props.filter)}>
              <Star selected={true}/> Saved
            </Button>
          </ButtonGroup>
          <ul className="message-list">
            {messages}
          </ul>
        </Card>
      </Col>
    </Row>
  )
}

Live.propTypes = {
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
        dispatch(setMessageFilter(filter))
      },
      onSaveClick: (message) => {
        dispatch(toggleSavedMessage(message))
      }
    }
  }
)(Live)
