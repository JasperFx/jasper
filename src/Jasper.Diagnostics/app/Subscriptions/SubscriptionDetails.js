import {
  default as React,
  PropTypes
} from 'react'
import Card from '../Components/Card'
import ItemDetail from '../Components/ItemDetail'

const SubscriptionDetails = ({subscription}) => {
  return (
    <Card>
      <ItemDetail label="Service Name" value={subscription.serviceName}/>
      <ItemDetail label="Destination" value={subscription.destination}/>
      <ItemDetail label="Message Type" value={subscription.messageType}/>
      <ItemDetail label="Accept" value={subscription.accept.toString()}/>
    </Card>
  )
}

SubscriptionDetails.propTypes = {
  subscription: PropTypes.object.isRequired
}

export default SubscriptionDetails
