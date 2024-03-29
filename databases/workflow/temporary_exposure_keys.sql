CREATE TABLE public.temporary_exposure_keys
(
    id bigint NOT NULL GENERATED BY DEFAULT AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 9223372036854775807 CACHE 1 ),
    owner_id bigint NOT NULL,
    key_data bytea NOT NULL,
    rolling_start_number integer NOT NULL,
    rolling_period integer NOT NULL,
    publishing_state integer NOT NULL,
    publish_after timestamp with time zone NOT NULL,
    CONSTRAINT pk_temporary_exposure_keys PRIMARY KEY (id),
    CONSTRAINT fk_temporary_exposure_keys_key_release_workflow_states_owner_id FOREIGN KEY (owner_id)
        REFERENCES public.key_release_workflow_states (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
);

CREATE INDEX ix_temporary_exposure_keys_owner_id
    ON public.temporary_exposure_keys USING btree
    (owner_id ASC NULLS LAST);

CREATE INDEX ix_temporary_exposure_keys_publish_after
    ON public.temporary_exposure_keys USING btree
    (publish_after ASC NULLS LAST);

CREATE INDEX ix_temporary_exposure_keys_publishing_state
    ON public.temporary_exposure_keys USING btree
    (publishing_state ASC NULLS LAST);
