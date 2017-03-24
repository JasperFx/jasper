import {
  default as React,
  PropTypes
} from 'react'
import cn from 'classnames'
import AwesomeIcon from './Components/AwesomeIcon'
import Container from './Components/Container'
import Row from './Components/Row'
import Col from './Components/Col'
import './TitleBar.css'

function TitleBar(props) {
  const style = cn('title-bar', props.className)
  return (
    <div className={style}>
      <Container>
        <Row>
          <Col column={12}>
            <AwesomeIcon icon="signal" className="title-bar-icon"/> <span className="title-bar-title">Jasper Diagnostics</span>
          </Col>
        </Row>
      </Container>
    </div>
  )
}

TitleBar.propTypes = {
  className: PropTypes.string,
  children: PropTypes.any
}

export default TitleBar
