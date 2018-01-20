CREATE TABLE public.payment_types
(
    id UUID PRIMARY KEY,
    instance_id UUID NOT NULL,
    name CITEXT,
    CONSTRAINT payment_types_instances_id_fk FOREIGN KEY (instance_id) REFERENCES instances (id)
);