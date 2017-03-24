import {
  default as React,
  PropTypes
} from 'react'
import { connect } from 'react-redux'
import cn from 'classnames'
import AwesomeIcon from '../Components/AwesomeIcon'
import Button from '../Components/Button'
import ButtonGroup from '../Components/ButtonGroup'
import Row from '../Components/Row'
import Col from '../Components/Col'
import Card from '../Components/Card'
import {
  setMessageFilter,
  getVisibleMessages
} from './liveMessagesReducer'
import './index.css'

function checkIfSelected(mode, selectedMode) {
  return mode === selectedMode
}

function Live(props) {
  const messages = props.messages.map((m, idx) => {
    const status = cn(m.hasError ? 'danger' : 'success', 'message-status')
    return (
      <li key={idx} className="message-list-item"><AwesomeIcon icon="circle" className={status}/><AwesomeIcon icon="star-o"/>{m.description}</li>
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
              <AwesomeIcon icon="circle" className="success"/> Success
            </Button>
            <Button
              click={()=>props.onFilterClick('failed')}
              selected={checkIfSelected('failed', props.filter)}>
              <AwesomeIcon icon="circle" className="danger"/> Failed
            </Button>
            <Button
              click={()=>props.onFilterClick('saved')}
              selected={checkIfSelected('saved', props.filter)}>
              <AwesomeIcon icon="star" className="warning"/> Saved
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
  onFilterClick: PropTypes.func.isRequired
}

export default connect(
  (state) => {
    return {
      filter: state.live.filter,
      messages: getVisibleMessages(state.live.messages, state.live.filter)
    }
  },
  (dispatch) => {
    return {
      onFilterClick: (filter) => {
        dispatch(setMessageFilter(filter))
      }
    }
  }
)(Live)
