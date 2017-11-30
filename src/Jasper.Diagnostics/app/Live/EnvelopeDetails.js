import {
  default as React,
  PropTypes
} from 'react'
import { connect } from 'react-redux'
import Card from '../Components/Card'
import Code from '../Components/Code'
import Row from '../Components/Row'
import Col from '../Components/Col'
import StatusIndicator from '../Components/StatusIndicator'
import AwesomeIcon from '../Components/AwesomeIcon'
import './EnvelopeDetails.css'
import ItemDetail from '../Components/ItemDetail'

const NoMessage = () => {
  return (
    <Card>
      Message not found
    </Card>
  )
}

const EnvelopeError = ({exception, stackTrace}) => {
  return (
    <Card>
        <Row>
          <Col column={12}>
            <h2 className="header-title">Exception</h2>
          </Col>
        </Row>
        <Row>
          <Col column={12}>
            <p className="danger">{exception}</p>
          </Col>
        </Row>
        <Row>
          <Col column={12}>
            <Code>{stackTrace}</Code>
          </Col>
        </Row>
    </Card>
  )
}

EnvelopeError.propTypes = {
  exception: PropTypes.string.isRequired,
  stackTrace: PropTypes.string.isRequired
}

const EnvelopeDetails = ({ message, goBack }) => {

  if (message == null) {
    return <NoMessage/>
  }

  const error = message.hasError ? <EnvelopeError exception={message.exception} stackTrace={message.stackTrace}/> : null
  const back = ev => {
    ev.preventDefault()
    goBack()
  }

  //creating a version of the message that removes the 'extra' parameters that are added to the message for convience.
  const displayMessage = {...message};
  delete displayMessage.saved
  delete displayMessage.hasError
  return (
    <div>
      <Row>
        <Col column={12}>
          <a href="" onClick={back} className="back-nav"><AwesomeIcon icon="arrow-left"/>Back</a>
        </Col>
      </Row>
      <Row>
        <Col column={6}>
          <Card>
            <h2 className="header-title">Message Summary <StatusIndicator success={!message.hasError}/></h2>
            <ItemDetail label="CorrelationId" value={message.correlationId}/>
            <ItemDetail label="Type" value={message.messageType.name}/>
            <ItemDetail label="Destination" value={message.headers.destination}/>
            <ItemDetail label="Received At" value={message.headers['received-at']}/>
            <ItemDetail label="Reply Uri" value={message.headers['reply-uri']}/>
            <ItemDetail label="Attempts" value={message.headers.attempts}/>
          </Card>
        </Col>
      </Row>
      <Row>
        <Col column={12}>
          <Card>
            <h2 className="header-title">Message Details</h2>
            <Code>{JSON.stringify(displayMessage, null, ' ')}</Code>
          </Card>
        </Col>
      </Row>
      {error}
    </div>
  )
}

EnvelopeDetails.propTypes = {
  message: PropTypes.shape({
    description: PropTypes.string.isRequired
  }),
  goBack: PropTypes.func.isRequired
}

export default connect(
  (state, props)=> {
    return {
      message: state.messages[props.match.params.direction + "Messages"].find(m => m.correlationId === props.match.params.id)
    }
  },
  (dispatch, props)=> {
    return {
      goBack: ()=> {
        props.history.goBack()
      }
    }
  }
 )(EnvelopeDetails)
