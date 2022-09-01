CREATE TABLE public.input
(
    id bigint NOT NULL GENERATED BY DEFAULT AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 9223372036854775807 CACHE 1 ),
    dk_id bigint NOT NULL,
    used boolean NOT NULL,
    daily_key_key_data bytea,
    daily_key_rolling_start_number integer,
    daily_key_rolling_period integer,
    transmission_risk_level integer NOT NULL,
    report_type integer NOT NULL,
    countries_of_interest text NOT NULL,
    days_since_symptoms_onset integer NOT NULL,
    CONSTRAINT pk_input PRIMARY KEY (id)
);
