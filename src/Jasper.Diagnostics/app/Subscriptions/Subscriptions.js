import {connect} from 'react-redux'
import {default as React, PropTypes} from 'react'
import {default as SubDetails } from './SubscriptionDetails.js'

const Subscriptions = ({subscriptions}) => {
  const list = subscriptions.map((s, idx) => <SubDetails key={idx} subscription={s}/>)
  return <div>
      {list}
  </div>
}

Subscriptions.propTypes = {
  subscriptions: PropTypes.array.isRequired
}

const mapStateToProps = state => {
  return {
    subscriptions: state.subscriptionInfo.subscriptions
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
