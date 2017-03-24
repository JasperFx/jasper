import {
  default as React,
  PropTypes
} from 'react'

function Col(props) {
  const className = `col-md-${props.column}`
  return (
    <div className={className}>
      {props.children}
    </div>
  )
}

Col.propTypes = {
  column: PropTypes.number,
  children: PropTypes.any
}

export default Col
