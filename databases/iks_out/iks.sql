
CREATE TABLE public.iks
(
    id integer NOT NULL GENERATED BY DEFAULT AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
    created timestamp with time zone NOT NULL,
    valid_for timestamp with time zone NOT NULL,
    content bytea NOT NULL,
    sent boolean NOT NULL,
    qualifier integer NOT NULL,
    error boolean NOT NULL DEFAULT 0,
    process_state text NOT NULL DEFAULT 'New',
    retry_count integer NOT NULL DEFAULT 0,
    can_retry boolean,
    CONSTRAINT pk_iks PRIMARY KEY (id)
);
