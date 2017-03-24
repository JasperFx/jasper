import {
  default as React,
  PropTypes
} from 'react'

function Container(props) {
  return (
    <div className="container">
      {props.children}
    </div>
  )
}

Container.propTypes = {
  children: PropTypes.any
}

export default Container
