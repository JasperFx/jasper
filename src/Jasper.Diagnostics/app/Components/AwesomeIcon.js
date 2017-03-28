import {
  default as React,
  PropTypes
} from 'react'
import cn from 'classnames'

function AwesomeIcon({icon, className, onClick}) {
  const styles = cn('fa', `fa-${icon}`, className)
  return (
    <i className={styles} aria-hidden="true" onClick={onClick}></i>
  )
}

AwesomeIcon.propTypes = {
  icon: PropTypes.string.isRequired,
  className: PropTypes.string,
  onClick: PropTypes.func
}

export default AwesomeIcon
