CREATE TABLE public.warehouses
(
    id UUID PRIMARY KEY,
    instance_id UUID NOT NULL,
    vendor_code CITEXT,
    name CITEXT,
    raw_address CITEXT,
    geoposition GEOGRAPHY(POINT),
    CONSTRAINT products_instances_id_fk FOREIGN KEY (instance_id) REFERENCES instances (id)
);
