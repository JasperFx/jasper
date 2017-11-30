import {
  default as React,
  PropTypes
} from 'react'
import { connect } from 'react-redux'
import Card from '../Components/Card'
import Code from '../Components/Code'
import AwesomeIcon from '../Components/AwesomeIcon'
import Row from '../Components/Row'
import Col from '../Components/Col'
import RecentMessages from '../Live/RecentMessages'
import {RECV} from "../Live/messagesReducer"

const HandlerChainDetails = ({ chain, goBack }) => {
  const back = ev => {
    ev.preventDefault()
    goBack()
  }
  const { messageType, description, sourceCode } = chain
  return (
    <div>
      <Row>
        <Col column={12}>
          <a href="" onClick={back} className="back-nav"><AwesomeIcon icon="arrow-left"/>Back</a>
        </Col>
      </Row>
      <Row>
        <Col column={8}>
        <Card>
          <h2 className="header-title no-transform">{messageType.fullName}</h2>
          <div>{description}</div>
          <Code className="language-javascript">{sourceCode}</Code>
        </Card>
        </Col>
        <Col column={4}>
          <Card>
            <h2 className="header-title">Recent Messages</h2>
            <RecentMessages typeName={chain.messageType.fullName} direction={RECV}/>
          </Card>
        </Col>
      </Row>
    </div>
  )
}

HandlerChainDetails.propTypes = {
  chain: PropTypes.object.isRequired,
  goBack: PropTypes.func.isRequired
}

export default connect((state, props) => {
    return {
      chain: state.capabilities.chains.find(e => e.generatedTypeName === props.match.params.id)
    }
  },
  (dispatch, props)=> {
    return {
      goBack: ()=> {
        props.history.goBack()
      }
    }
  }
)(HandlerChainDetails)
