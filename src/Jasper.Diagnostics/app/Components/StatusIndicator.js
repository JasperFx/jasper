import {
  default as React,
  PropTypes
} from 'react'
import AwesomeIcon from './AwesomeIcon'

function StatusIndicator({ success }) {
  const status = success === true ? 'success' : 'danger'
  return (
    <AwesomeIcon icon="circle" className={status}/>
  )
}

StatusIndicator.propTypes = {
  success: PropTypes.bool
}

export default StatusIndicator
