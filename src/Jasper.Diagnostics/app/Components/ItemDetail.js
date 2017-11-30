import { default as React, PropTypes } from 'react'

const ItemDetail = ({label, value}) => {
  return (
    <div><span className="item-label">{label}:</span> {value}</div>
  )
}

ItemDetail.propTypes = {
  label: PropTypes.string.isRequired,
  value: PropTypes.string
}

export default ItemDetail
