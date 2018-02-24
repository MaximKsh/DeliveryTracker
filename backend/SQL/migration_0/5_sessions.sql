CREATE TABLE public.sessions
(
    id UUID PRIMARY KEY NOT NULL,
    user_id UUID NOT NULL,
    session_token_id UUID NULL,
    refresh_token_id UUID NULL,
    last_activity TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT ( now() AT TIME ZONE 'UTC'),
    CONSTRAINT sessions_users_id_fk FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
);
CREATE UNIQUE INDEX sessions_user_id_uindex ON public.sessions (user_id);