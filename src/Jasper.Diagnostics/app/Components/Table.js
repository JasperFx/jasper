import {
  default as React,
  PropTypes
} from 'react'
import './Table.css'

function Table(props) {
  return (
    <div className="table-responsive">
      <table className="table table-hover">
        {props.children}
      </table>
    </div>
  )
}

Table.propTypes = {
  children: PropTypes.any,
  head: PropTypes.any
}

function Head(props) {
  return (
    <thead>
      {props.children}
    </thead>
  )
}

Head.propTypes = {
  children: PropTypes.any
}

function Body(props) {
  return (
    <tbody>
      {props.children}
    </tbody>
  )
}

Body.propTypes = {
  children: PropTypes.any
}

export { Head, Body }

export default Table
