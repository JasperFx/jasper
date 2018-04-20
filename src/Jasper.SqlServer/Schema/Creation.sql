
create table %SCHEMA%.jasper_outgoing_envelopes
(
	id uniqueidentifier not null primary key,
	owner_id int not null,
	destination varchar(250) not null,
	deliver_by datetimeoffset,
	body varbinary(max) not null
);

create table %SCHEMA%.jasper_incoming_envelopes
(
	id uniqueidentifier not null
		primary key,
	status varchar(25) not null,
	owner_id int not null,
	execution_time datetimeoffset default NULL,
	attempts int default 0 not null,
	body varbinary(max) not null
);




