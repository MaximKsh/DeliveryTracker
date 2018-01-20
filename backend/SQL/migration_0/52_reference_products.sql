CREATE TABLE public.products
(
    id UUID PRIMARY KEY,
    instance_id UUID NOT NULL,
    vendor_code CITEXT,
    name CITEXT,
    description CITEXT,
    cost decimal,
    CONSTRAINT products_instances_id_fk FOREIGN KEY (instance_id) REFERENCES instances (id)
);
