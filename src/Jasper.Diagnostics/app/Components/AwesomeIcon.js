import {
  default as React,
  PropTypes
} from 'react'
import cn from 'classnames'

function AwesomeIcon(props) {
  const styles = cn('fa', `fa-${props.icon}`, props.className)
  return (
    <i className={styles} aria-hidden="true"></i>
  )
}

AwesomeIcon.propTypes = {
  icon: PropTypes.string,
  className: PropTypes.string
}

export default AwesomeIcon
