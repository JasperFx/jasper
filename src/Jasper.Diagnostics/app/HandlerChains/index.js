import {
  default as React,
  PropTypes
} from 'react'
import { connect } from 'react-redux'
import { withRouter } from 'react-router-dom'
import {
  default as Table,
  Head,
  Body,
} from '../Components/Table'
import Card from '../Components/Card'
import './index.css'

function HandlerChain (props) {
      //<Code className="language-javascript">{props.sourceCode}</Code>
  return (
    <tr className="table-row" onClick={() => props.click(props.chain)}>
      <td>
        {props.chain.messageType.fullName}
      </td>
      <td>
        {props.chain.description}
      </td>
    </tr>
  )
}

HandlerChain.propTypes = {
  click: PropTypes.func.isRequired,
  chain: PropTypes.shape({
    description: PropTypes.string,
    generatedTypeName: PropTypes.string,
    sourceCode: PropTypes.string,
    messageType: PropTypes.shape({
      fullName: PropTypes.string
    })
  })
}

function HandlerChains({ chains, history }) {

  const click = (c)=> {
    history.push(`/handler-chain/${c.generatedTypeName}`)
  }

  const list = chains.map((c, i) => <HandlerChain key={i} chain={c} click={click}/>)
  return (
    <Card>
      <h2 className="header-title">Handler Chains</h2>
      <Table>
        <Head>
          <tr>
            <td>Message</td>
            <td>Description</td>
          </tr>
        </Head>
        <Body>{list}</Body>
      </Table>
    </Card>
  )
}

HandlerChains.propTypes = {
  chains: PropTypes.array.isRequired,
  history: PropTypes.shape({
    push: PropTypes.func.isRequired
  })
}

export default connect(
  (state) => {
    return {
      chains: state.handlerChains.chains
    }
  }
)(HandlerChains)
