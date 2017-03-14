var uri = "ws://" + window.location.host + "/_diag/ws";
function connect() {
  socket = new WebSocket(uri);
  socket.onopen = function(event) {
    console.log("opened connection to " + uri);
  };
  socket.onclose = function(event) {
    console.log("closed connection from " + uri);
  };
  socket.onmessage = function(event) {
    console.log(event.data);
  };
  socket.onerror = function(event) {
    console.log("error: ", event);
  };
}
connect();

console.log('ws');
