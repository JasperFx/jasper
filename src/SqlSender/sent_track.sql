create table sender.sent_track
(
	id uniqueidentifier not null primary key,
	message_type varchar(250) not null,
);

create table sender.received_track
(
	id uniqueidentifier not null primary key,
	message_type varchar(250) not null,
);
