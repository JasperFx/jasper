import {
  default as React,
  PropTypes
} from 'react'
import {
  NavLink
} from 'react-router-dom'
import './TopNavItem.css'

const TopNavItem = ({ exact, to, children }) => {
  const val = true === exact ? {exact:true} : null
  return (
    <NavLink {...val} to={to} activeClassName="top-nav-item-active">
      {children}
    </NavLink>
  )
}

TopNavItem.propTypes = {
  to: PropTypes.string.isRequired,
  children: PropTypes.any.isRequired,
  exact: PropTypes.bool
}

export default TopNavItem
