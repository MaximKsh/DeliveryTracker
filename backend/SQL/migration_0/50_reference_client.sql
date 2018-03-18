CREATE TABLE public.clients
(
    id UUID PRIMARY KEY,
    instance_id UUID NOT NULL,
    deleted BOOLEAN NOT NULL DEFAULT FALSE,
    surname CITEXT,
    name CITEXT,
    patronymic CITEXT,
    phone_number VARCHAR(20),
    search citext,
    CONSTRAINT clients_instances_id_fk FOREIGN KEY (instance_id) REFERENCES instances (id)
);

CREATE TABLE public.client_addresses
(
    id UUID PRIMARY KEY,
    instance_id UUID NOT NULL,
    deleted BOOLEAN NOT NULL DEFAULT FALSE,
    parent_id UUID,
    raw_address CITEXT,
    geoposition GEOGRAPHY(POINT),
    CONSTRAINT addresses_instances_id_fk FOREIGN KEY (instance_id) REFERENCES instances (id),
    CONSTRAINT addresses_parent_id_fk FOREIGN KEY (parent_id) REFERENCES clients (id)
);


CREATE OR REPLACE FUNCTION insert_clients_statistics()
    RETURNS TRIGGER AS $entries_statistics$
BEGIN
    UPDATE entries_statistics
    SET clients_count = clients_count + 1
    WHERE instance_id = NEW.instance_id;
    RETURN NEW;
END;
$entries_statistics$
LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION delete_clients_statistics()
    RETURNS TRIGGER AS $entries_statistics$
BEGIN
    UPDATE entries_statistics
    SET clients_count = clients_count - 1
    WHERE instance_id = NEW.instance_id;
    RETURN NEW;
END;
$entries_statistics$
LANGUAGE plpgsql;

CREATE TRIGGER insert_clients_statistics_trigger
AFTER INSERT ON clients FOR EACH ROW EXECUTE PROCEDURE insert_clients_statistics();

CREATE TRIGGER delete_clients_statistics_trigger
AFTER UPDATE ON clients FOR EACH ROW EXECUTE PROCEDURE delete_clients_statistics();


CREATE OR REPLACE FUNCTION insert_clients_search()
    RETURNS TRIGGER AS $clients$
BEGIN
    NEW.search := coalesce(NEW.surname, '')
                  || ' '
                  || coalesce(NEW.name, '')
                  || ' '
                  || coalesce(NEW.patronymic, '')
                  || ' '
                  || coalesce(NEW.phone_number, '');
    RETURN NEW;
END;
$clients$
LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_clients_search()
    RETURNS TRIGGER AS $clients$
BEGIN
    NEW.search := coalesce(NEW.surname, OLD.surname, '')
                  || ' '
                  || coalesce(NEW.name, OLD.name, '')
                  || ' '
                  || coalesce(NEW.patronymic, OLD.patronymic, '')
                  || ' '
                  || coalesce(NEW.phone_number, OLD.phone_number, '');
    RETURN NEW;
END;
$clients$
LANGUAGE plpgsql;

CREATE TRIGGER insert_clients_search_trigger
BEFORE INSERT ON clients FOR EACH ROW EXECUTE PROCEDURE insert_clients_search();

CREATE TRIGGER update_clients_search_trigger
BEFORE UPDATE ON clients FOR EACH ROW EXECUTE PROCEDURE update_clients_search();