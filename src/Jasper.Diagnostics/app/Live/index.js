import {
  default as React,
} from 'react'
import Row from '../Components/Row'
import Col from '../Components/Col'
import Card from '../Components/Card'
import SentMessages from './SentMessages'
import LiveMessages from './LiveMessages'
import './index.css'

const Live = (props) => {
  return (
    <div>
      <Row>
        <Col column={12}>
          <Card>
            <h2 className="header-title">Received Messages</h2>
            <LiveMessages/>
          </Card>
        </Col>
      </Row>
      <Row>
        <Col column={12}>
          <Card>
            <h2 className="header-title">Sent Messages</h2>
            <SentMessages/>
          </Card>
        </Col>
      </Row>
    </div>
  )
}

export default Live
