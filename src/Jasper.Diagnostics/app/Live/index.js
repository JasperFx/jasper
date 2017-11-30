import { default as React } from 'react'
import Row from '../Components/Row'
import Col from '../Components/Col'
import Card from '../Components/Card'
import Messages from './Messages'
import './index.css'
import {
  RECV,
  SENT
} from './messagesReducer'

const Live = (props) => {
  return (
    <Row>
      <Col column={6}>
        <Card>
          <h2 className="header-title">Received Messages</h2>
          <Messages direction={RECV}/>
        </Card>
      </Col>
        <Col column={6}>
          <Card>
            <h2 className="header-title">Sent Messages</h2>
            <Messages direction={SENT}/>
          </Card>
        </Col>
    </Row>
  )
}
export default Live
