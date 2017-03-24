import {
  default as React,
  PropTypes
} from 'react'

function Row(props) {
  return (
    <div className="row">
      {props.children}
    </div>
  )
}

Row.propTypes = {
  children: PropTypes.any
}

export default Row
