CREATE TABLE public.users
(
    id UUID PRIMARY KEY,
    code VARCHAR(10) NOT NULL,
    role VARCHAR(50) NOT NULL,
    instance_id UUID NOT NULL,
    password_hash TEXT, 
    surname CITEXT,
    name CITEXT,
    patronymic CITEXT,
    phone_number VARCHAR(20),
    CONSTRAINT users_instances_id_fk FOREIGN KEY (instance_id) REFERENCES instances (id)
);
CREATE UNIQUE INDEX users_code_uindex ON public.users (code);