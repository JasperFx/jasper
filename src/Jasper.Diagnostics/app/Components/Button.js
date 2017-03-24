import {
  default as React,
  PropTypes
} from 'react'
import cn from 'classnames'
import './Button.css'

function Button(props) {
  const type = props.type || 'default'
  const style = cn(
    'btn',
    `btn-${type}`,
    props.selected ? 'btn-selected' : null,
    props.className
  )
  return (
    <button type="button" className={style} onClick={props.click}>
      {props.children}
    </button>
  )
}

Button.propTypes = {
  className: PropTypes.string,
  children: PropTypes.any,
  type: PropTypes.oneOf(['default', 'warning', 'danger']),
  click: PropTypes.func,
  selected: PropTypes.bool
}

export default Button
