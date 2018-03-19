CREATE TABLE public.warehouses
(
    id UUID PRIMARY KEY,
    instance_id UUID NOT NULL,
    deleted BOOLEAN NOT NULL DEFAULT FALSE,
    vendor_code CITEXT,
    name CITEXT,
    raw_address CITEXT,
    geoposition GEOGRAPHY(POINT),
    search CITEXT,
    CONSTRAINT products_instances_id_fk FOREIGN KEY (instance_id) REFERENCES instances (id)
);

CREATE OR REPLACE FUNCTION insert_warehouses_statistics()
    RETURNS TRIGGER AS $entries_statistics$
BEGIN
    UPDATE entries_statistics
    SET warehouses_count = warehouses_count + 1
    WHERE instance_id = NEW.instance_id;
    RETURN NEW;
END;
$entries_statistics$
LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION delete_warehouses_statistics()
    RETURNS TRIGGER AS $entries_statistics$
BEGIN
    IF (OLD.deleted = FALSE AND NEW.deleted = TRUE) THEN
        UPDATE entries_statistics
        SET warehouses_count = warehouses_count - 1
        WHERE instance_id = NEW.instance_id;
    END IF;
    RETURN NEW;
END;
$entries_statistics$
LANGUAGE plpgsql;

CREATE TRIGGER insert_warehouses_statistics_trigger
AFTER INSERT ON warehouses FOR EACH ROW EXECUTE PROCEDURE insert_warehouses_statistics();

CREATE TRIGGER delete_warehouses_statistics_trigger
AFTER UPDATE ON warehouses FOR EACH ROW EXECUTE PROCEDURE delete_warehouses_statistics();


CREATE OR REPLACE FUNCTION insert_warehouses_search()
    RETURNS TRIGGER AS $warehouses$
BEGIN
    NEW.search := coalesce(NEW.raw_address, '')
                  || ' '
                  || coalesce(NEW.name, '');
    RETURN NEW;
END;
$warehouses$
LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_warehouses_search()
    RETURNS TRIGGER AS $warehouses$
BEGIN
    NEW.search := coalesce(NEW.raw_address, OLD.raw_address, '')
                  || ' '
                  || coalesce(NEW.name, OLD.name, '');
    RETURN NEW;
END;
$warehouses$
LANGUAGE plpgsql;

CREATE TRIGGER insert_warehouses_search_trigger
BEFORE INSERT ON warehouses FOR EACH ROW EXECUTE PROCEDURE insert_warehouses_search();

CREATE TRIGGER update_warehouses_search_trigger
BEFORE UPDATE ON warehouses FOR EACH ROW EXECUTE PROCEDURE update_warehouses_search();