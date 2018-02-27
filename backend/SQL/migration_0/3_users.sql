CREATE TABLE public.users
(
    id UUID PRIMARY KEY,
    code VARCHAR(10) NOT NULL,
    role UUID NOT NULL,
    instance_id UUID NOT NULL,
    last_activity TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT ( now() AT TIME ZONE 'UTC' ),
    password_hash TEXT,
    surname CITEXT,
    name CITEXT,
    patronymic CITEXT,
    phone_number VARCHAR(20),
    geoposition GEOGRAPHY(POINT),
    CONSTRAINT users_roles_role_fk FOREIGN KEY (role) REFERENCES roles (id),
    CONSTRAINT users_instances_id_fk FOREIGN KEY (instance_id) REFERENCES instances (id)
);
CREATE UNIQUE INDEX users_code_uindex ON public.users (code);