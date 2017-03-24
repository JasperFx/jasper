/* global Prism */
// https://github.com/tomchentw/react-prism

import {
  default as React,
  PropTypes,
  PureComponent
} from 'react'

export default class Code extends PureComponent {

  static propTypes = {
    async: PropTypes.bool,
    className: PropTypes.string,
    children: PropTypes.any,
  }

  componentDidMount() {
    this._hightlight()
  }

  componentDidUpdate() {
    this._hightlight()
  }

  _hightlight() {
    Prism.highlightElement(this.refs.code, this.props.async)
  }

  render() {
    const { className, children } = this.props
    return (
      <pre>
        <code ref="code" className={className}>
          {children}
        </code>
      </pre>
    )
  }
}
