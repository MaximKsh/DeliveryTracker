CREATE TABLE public.products
(
    id UUID PRIMARY KEY,
    instance_id UUID NOT NULL,
    deleted BOOLEAN NOT NULL DEFAULT FALSE,
    vendor_code CITEXT,
    name CITEXT,
    description CITEXT,
    cost decimal,
    CONSTRAINT products_instances_id_fk FOREIGN KEY (instance_id) REFERENCES instances (id)
);

CREATE OR REPLACE FUNCTION insert_products_statistics()
    RETURNS TRIGGER AS $entries_statistics$
BEGIN
    UPDATE entries_statistics
    SET products_count = products_count + 1
    WHERE instance_id = NEW.instance_id;
    RETURN NEW;
END;
$entries_statistics$
LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION delete_products_statistics()
    RETURNS TRIGGER AS $entries_statistics$
BEGIN
    UPDATE entries_statistics
    SET products_count = products_count - 1
    WHERE instance_id = NEW.instance_id;
    RETURN NEW;
END;
$entries_statistics$
LANGUAGE plpgsql;

CREATE TRIGGER insert_products_statistics_trigger
AFTER INSERT ON products FOR EACH ROW EXECUTE PROCEDURE insert_products_statistics();

CREATE TRIGGER delete_products_statistics_trigger
AFTER UPDATE ON products FOR EACH ROW EXECUTE PROCEDURE delete_products_statistics();