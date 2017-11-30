import {
  default as React,
  PropTypes
} from 'react'
import './index.css'
import HandlerChains from './HandlerChains'
import Publications from './Publications'
import Subscriptions from "./Subscriptions"

const Capabilities = () => {
  return (
    <div>
      <HandlerChains/>
      <Publications/>
      <Subscriptions/>
    </div>
  )
}

export default Capabilities
