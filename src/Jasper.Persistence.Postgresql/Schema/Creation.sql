
create table %SCHEMA%.jasper_outgoing_envelopes
(
	id uuid not null primary key,
	owner_id int not null,
	destination varchar(250) not null,
	deliver_by timestamptz,
	body bytea not null
);

create table if not exists %SCHEMA%.jasper_incoming_envelopes
(
	id uuid not null
		primary key,
	status varchar(25) not null,
	owner_id int not null,
	execution_time timestamptz default NULL,
	attempts int default 0 not null,
	body bytea not null
);

create table if not exists %SCHEMA%.jasper_dead_letters
(
	id uuid not null
		primary key,

  source VARCHAR(250),
  message_type VARCHAR(250),
  explanation VARCHAR(250),
  exception_text VARCHAR(MAX),
  exception_type VARCHAR(250),
  exception_message VARCHAR(MAX),

	body bytea not null
);








