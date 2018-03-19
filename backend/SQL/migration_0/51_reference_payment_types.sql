CREATE TABLE public.payment_types
(
    id UUID PRIMARY KEY,
    instance_id UUID NOT NULL,
    deleted BOOLEAN NOT NULL DEFAULT FALSE,
    name CITEXT,
    CONSTRAINT payment_types_instances_id_fk FOREIGN KEY (instance_id) REFERENCES instances (id)
);

CREATE OR REPLACE FUNCTION insert_payment_types_statistics()
    RETURNS TRIGGER AS $entries_statistics$
BEGIN
    UPDATE entries_statistics
    SET payment_types_count = payment_types_count + 1
    WHERE instance_id = NEW.instance_id;
    RETURN NEW;
END;
$entries_statistics$
LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION delete_payment_types_statistics()
    RETURNS TRIGGER AS $entries_statistics$
BEGIN
    IF (OLD.deleted = FALSE AND NEW.deleted = TRUE) THEN
        UPDATE entries_statistics
        SET payment_types_count = payment_types_count - 1
        WHERE instance_id = NEW.instance_id;
    END IF;
    RETURN NEW;
END;
$entries_statistics$
LANGUAGE plpgsql;

CREATE TRIGGER insert_payment_types_statistics_trigger
AFTER INSERT ON payment_types FOR EACH ROW EXECUTE PROCEDURE insert_payment_types_statistics();

CREATE TRIGGER delete_payment_types_statistics_trigger
AFTER UPDATE ON payment_types FOR EACH ROW EXECUTE PROCEDURE delete_payment_types_statistics();