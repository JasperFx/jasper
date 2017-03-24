import {
  default as React,
  PropTypes
} from 'react'
import cn from 'classnames'
import './Alert.css'

function Alert(props) {
  const type = `alert-${props.type}`
  const style = cn('alert', type, props.className)
  return (
    <div className={style} role="alert">
      {props.children}
    </div>
  )
}

Alert.propTypes = {
  type: PropTypes.oneOf(['success', 'info', 'warning', 'danger']).isRequired,
  children: PropTypes.any,
  className: PropTypes.string
}

export default Alert
