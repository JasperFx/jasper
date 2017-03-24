import {
  default as React,
  PropTypes
} from 'react'
import cn from 'classnames'
import './Card.css'

function Card(props) {
  const style = cn('card', props.className)
  return (
    <div className={style}>
      {props.children}
    </div>
  )
}

Card.propTypes = {
  className: PropTypes.string,
  children: PropTypes.any
}

export default Card
