import {
  default as React,
  PropTypes
} from 'react'
import cn from 'classnames'
import Container from './Components/Container'
import Row from './Components/Row'
import Col from './Components/Col'
import './TopNav.css'

const TopNav = ({ className, children }) => {
  const style = cn('top-nav', className)
  const items = React.Children.map(children, (c, idx) => {
    return (
      <li key={idx} className="top-nav-item">{c}</li>
    )
  })
  return (
    <div className={style}>
      <Container>
        <Row>
          <Col column={12}>
            <ul className="top-nav-list">
              {items}
            </ul>
          </Col>
        </Row>
      </Container>
    </div>
  )
}

TopNav.propTypes = {
  className: PropTypes.string,
  children: PropTypes.any
}

export default TopNav
