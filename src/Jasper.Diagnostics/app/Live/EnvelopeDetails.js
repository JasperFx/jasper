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
import './EnvelopeDetails.css'

function EnvelopeError({exception, stackTrace}) {
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

function ItemDetail({label, value}) {
  return (
    <div><span className="item-label">{label}:</span> {value}</div>
  )
}

function EnvelopeDetails({ message }) {
  const error = message.hasError ? <EnvelopeError exception={message.exception} stackTrace={message.stackTrace}/> : null
  return (
    <div>
      <Row>
        <Col column={6}>
          <Card>
            <h2 className="header-title">Envelope Details <StatusIndicator success={!message.hasError}/></h2>
            <ItemDetail label="CorrelationId" value={message.correlationId}/>
            <ItemDetail label="Type" value={message.messageType.fullName}/>
            <ItemDetail label="Destination" value={message.headers.destination}/>
            <ItemDetail label="Received At" value={message.headers['received-at']}/>
            <ItemDetail label="Reply Uri" value={message.headers['reply-uri']}/>
            <ItemDetail label="Attempts" value={message.headers.attempts}/>
          </Card>
        </Col>
        <Col column={6}>
          <Card>
            <h2 className="header-title">Raw Headers</h2>
            <Code className="language-json">{JSON.stringify(message.headers, null, ' ')}</Code>
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
  })
}

export default connect(
  (state, props)=> {
    return {
      message: state.live.messages.find(m => m.correlationId === props.match.params.id)
    }
  }
 )(EnvelopeDetails)
