import {
  default as React,
  PropTypes
} from 'react'
import { connect } from 'react-redux'
import { push } from 'react-router-redux'
import {
  default as Table,
  Head,
  Body,
} from '../Components/Table'
import Card from '../Components/Card'
import './index.css'

const HandlerChain = ({ chain, click }) => {
  return (
    <tr className="table-row" onClick={() => click(chain)}>
      <td>
        {chain.messageType.fullName}
      </td>
      <td>
        {chain.description}
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

const HandlerChains = ({ chains, onNavigate }) => {

  const list = chains.map((c, i) => <HandlerChain key={i} chain={c} click={()=> onNavigate(c.generatedTypeName)}/>)

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
  onNavigate: PropTypes.func.isRequired,
}

export default connect(
  (state) => {
    return {
      chains: state.capabilities.chains
    }
  },
  (dispatch) => {
    return {
      onNavigate: id => {
        dispatch(push(`/handler-chain/${id}`))
      }
    }
  }
)(HandlerChains)
