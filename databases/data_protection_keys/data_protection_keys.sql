CREATE TABLE public.data_protection_keys
(
    id integer NOT NULL GENERATED BY DEFAULT AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
    friendly_name text,
    xml text,
    CONSTRAINT pk_data_protection_keys PRIMARY KEY (id)
);
