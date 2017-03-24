import {
  default as React,
  PropTypes
} from 'react'
import cn from 'classnames'

function ButtonGroup(props) {
  const style = cn('btn-group', props.className)
  return (
    <div className={style} role="group">
      {props.children}
    </div>
  )
}

ButtonGroup.propTypes = {
  children: PropTypes.any,
  className: PropTypes.string
}

export default ButtonGroup
