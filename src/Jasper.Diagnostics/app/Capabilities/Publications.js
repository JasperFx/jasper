import {
  default as React,
  PropTypes
} from 'react'
import { connect } from 'react-redux'
import Card from "../Components/Card"
import ItemDetail from '../Components/ItemDetail'

const Publications = ({publications}) =>{
  return <Card>
      <h2 className="header-title">Publications</h2>
      {publications.map(publicationCard)}
    </Card>
}

const publicationCard = (pub,  index)=>{
  return <Card key={index}>
    {/*{JSON.stringify(pub)}*/}
    <ItemDetail label="Service Name" value={pub.serviceName} />
    <ItemDetail label="Message Type" value={pub.messageType} />
    <ItemDetail label="Content Types" value={pub.contentTypes.toString()} />
    <ItemDetail label="Transports" value={pub.transports.toString()} />
  </Card>
}


Publications.propTypes = {
  publications: PropTypes.array.isRequired,
}

export default connect(
  (state) => {
    return {
      publications: state.capabilities.publications
    }
  },
  (dispatch) => {
    return {}
  }
)(Publications)
