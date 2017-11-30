import {connect} from 'react-redux'
import {default as React, PropTypes} from 'react'
import {default as SubDetails } from '../Subscriptions/SubscriptionDetails.js'
import Card from "../Components/Card"

const Subscriptions = ({subscriptions}) => {
  const list = subscriptions.map((s, idx) => <SubDetails key={idx} subscription={s}/>)
  return <Card>
      <h2 className="header-title">Declared Subscriptions</h2>
      {list}
    </Card>
}

Subscriptions.propTypes = {
  subscriptions: PropTypes.array.isRequired
}

const mapStateToProps = state => {
  return {
    subscriptions: state.capabilities.declaredSubscriptions
  }
}

const mapDispatchToProps = dispatch => {
  return {}
}

const VisibleSubscriptions = connect(
  mapStateToProps,
  mapDispatchToProps
)(Subscriptions)

export default VisibleSubscriptions
