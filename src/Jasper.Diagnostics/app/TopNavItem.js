import {
  default as React,
  PropTypes
} from 'react'
import {
  NavLink
} from 'react-router-dom'
import './TopNavItem.css'

function TopNavItem(props) {
  const exact = true === props.exact ? {exact:true} : null
  return (
    <NavLink {...exact} to={props.to} activeClassName="top-nav-item-active">
      {props.children}
    </NavLink>
  )
}

TopNavItem.propTypes = {
  to: PropTypes.string.isRequired,
  children: PropTypes.any.isRequired,
  exact: PropTypes.bool
}

export default TopNavItem
