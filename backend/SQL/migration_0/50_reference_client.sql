CREATE TABLE public.clients
(
    id UUID PRIMARY KEY,
    instance_id UUID NOT NULL,
    surname CITEXT,
    name CITEXT,
    patronymic CITEXT,
    phone_number VARCHAR(20),
    CONSTRAINT clients_instances_id_fk FOREIGN KEY (instance_id) REFERENCES instances (id)
);

CREATE TABLE public.client_addresses
(
    id UUID PRIMARY KEY,
    instance_id UUID NOT NULL,
    parent_id UUID,
    raw_address CITEXT,
    geoposition GEOGRAPHY(POINT),
    CONSTRAINT addresses_instances_id_fk FOREIGN KEY (instance_id) REFERENCES instances (id),
    CONSTRAINT addresses_parent_id_fk FOREIGN KEY (parent_id) REFERENCES clients (id)
);