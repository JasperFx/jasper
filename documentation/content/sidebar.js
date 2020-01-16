$(document).ready(function(){
	var sidebar = $('#sidebar');

	$('#main-pane h2').each(function(i, h2){
		var id = 'sec' + i.toString();
		$(h2).attr('id', id);

		$('<li><a href="#' + id + '">' + $(h2).html() + '</a></li>').addClass("list-group-item").appendTo(sidebar);
	});
});