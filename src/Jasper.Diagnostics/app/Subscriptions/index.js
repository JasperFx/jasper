import {default as React} from 'react'
import './index.css'
import Card from '../Components/Card'
import {default as Subscriptions} from './Subscriptions'

const LiveIndex = () => {
  return (
          <Card>
            <h2 className="header-title"> Persisted Subscriptions</h2>
            <Subscriptions />
          </Card>
  )
}
export default LiveIndex
