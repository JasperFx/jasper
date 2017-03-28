import {
  default as React,
  PropTypes
} from 'react'
import cn from 'classnames'
import AwesomeIcon from './AwesomeIcon'

function Star({ selected, className, onClick }) {
  const icon = selected === true ? 'star' : 'star-o'
  const color = selected === true ? 'warning' : ''
  const style = cn(color, className)
  return (
    <AwesomeIcon icon={icon} className={style} onClick={onClick}/>
  )
}

Star.propTypes = {
  selected: PropTypes.bool,
  className: PropTypes.string,
  onClick: PropTypes.func
}

export default Star
