import {
  default as React,
  Component,
  PropTypes
} from 'react'
import {
  Route,
  withRouter
} from 'react-router-dom'
import { connect } from 'react-redux'
import Alert from './Components/Alert'
import Container from './Components/Container'
import Row from './Components/Row'
import Col from './Components/Col'
import AwesomeIcon from './Components/AwesomeIcon'
import TitleBar from './TitleBar'
import TopNav from './TopNav'
import TopNavItem from './TopNavItem'
import Capabilities from './Capabilities/index'
import HandlerChainDetails from './Capabilities/HandlerChainDetails'
import Live from './Live/index'
import EnvelopeDetails from './Live/EnvelopeDetails'
import './App.css'
import Subscriptions from './Subscriptions/'

const Routes = ({ alert }) => {
  return (
    <div>
      {alert}
      <TitleBar/>
      <TopNav>
        <TopNavItem to="/" exact><AwesomeIcon icon="share-alt"/> Capabilities</TopNavItem>
        <TopNavItem to="/live"><AwesomeIcon icon="envelope"/> Live Messages</TopNavItem>
        <TopNavItem to="/subscriptions"><AwesomeIcon icon="exchange"/> Subscriptions</TopNavItem>
      </TopNav>
      <Container>
        <Row>
          <Col column={12}>
            <Route exact path="/" component={Capabilities}/>
            <Route path="/subscriptions" component={Subscriptions} />
            <Route path="/live" component={Live}/>
            <Route path="/envelope/:direction/:id" component={EnvelopeDetails}/>
            <Route path="/handler-chain/:id" component={HandlerChainDetails}/>
          </Col>
        </Row>
      </Container>
    </div>
  )
}

Routes.propTypes = {
  alert: PropTypes.element
}

class App extends Component {
  render() {

    const { mode } = this.props
    let alert = null

    if (mode === 'disconnected') {
      alert = (
        <Row>
          <Col column={12}>
            <Alert type="danger" className="top-alert">
              <p className="text-center"><AwesomeIcon icon="warning"/> Jasper Diagnostics has been disconnected from the server.  Please refresh the browser to reconnect.</p>
            </Alert>
          </Col>
        </Row>
      )
    }

    return (<Routes {...this.props} alert={alert}/>)
  }
}

App.propTypes = {
  mode: PropTypes.string
}

export default withRouter(connect(state => {
  return state.app
})(App))
