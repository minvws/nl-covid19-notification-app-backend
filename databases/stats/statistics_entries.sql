CREATE TABLE public.statistics_entries
(
    id integer NOT NULL GENERATED BY DEFAULT AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
    created timestamp with time zone NOT NULL,
    name text NOT NULL,
    value double precision NOT NULL,
    total_from_portal double precision NOT NULL DEFAULT 0,
    total_from_other double precision NOT NULL DEFAULT 0,
    publishing_state integer NOT NULL,
    CONSTRAINT pk_statistics_entries PRIMARY KEY (id)
)

CREATE INDEX ix_statistics_entries_created
    ON public.statistics_entries USING btree
    (created ASC NULLS LAST);

CREATE INDEX ix_statistics_entries_name
    ON public.statistics_entries USING btree
    (name ASC NULLS LAST);