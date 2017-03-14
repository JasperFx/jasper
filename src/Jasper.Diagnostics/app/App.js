  import React, { Component } from 'react'

  export default class App extends Component {

    componentDidMount() {
      const uri = this.props.settings.websocketAddress
      const that = this
      that.socket = new WebSocket(uri)
      that.socket.onopen = function(event) {
        console.log('opened connection to ' + uri)
      }
      that.socket.onclose = function(event) {
        console.log('closed connection from ' + uri)
      }
      that.socket.onmessage = function(event) {
        console.log(event.data)
      };
      that.socket.onerror = function(event) {
        console.log('error: ', event)
      };
  }

  render() {
    return (
      <div className="container">
        Hello from Jasper Diagnostics??
      </div>
    )
  }
}
