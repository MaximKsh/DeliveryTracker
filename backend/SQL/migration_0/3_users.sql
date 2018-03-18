CREATE TABLE public.users
(
    id UUID PRIMARY KEY,
    deleted BOOLEAN NOT NULL DEFAULT FALSE,
    code VARCHAR(30) NOT NULL,
    role UUID NOT NULL,
    instance_id UUID NOT NULL,
    last_activity TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT ( now() AT TIME ZONE 'UTC' ),
    password_hash TEXT,
    surname CITEXT,
    name CITEXT,
    patronymic CITEXT,
    phone_number VARCHAR(20),
    geoposition GEOGRAPHY(POINT),
    search CITEXT,
    CONSTRAINT users_roles_role_fk FOREIGN KEY (role) REFERENCES roles (id),
    CONSTRAINT users_instances_id_fk FOREIGN KEY (instance_id) REFERENCES instances (id)
);
CREATE UNIQUE INDEX users_code_uindex ON public.users (code);

CREATE OR REPLACE FUNCTION insert_users_statistics()
    RETURNS TRIGGER AS $entries_statistics$
BEGIN
    IF (NEW.role = 'ca4e3a74-86bb-4c6e-84b5-9e2da47d1b2e')
    THEN
        UPDATE entries_statistics
        SET managers_count = managers_count + 1
        WHERE instance_id = NEW.instance_id;
    ELSIF (NEW.role = 'aa522dd3-3a11-46a0-afa7-260b70609524')
    THEN
        UPDATE entries_statistics
        SET performers_count = performers_count + 1
        WHERE instance_id = NEW.instance_id;
    END IF;
    RETURN NEW;
END;
$entries_statistics$
LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION delete_users_statistics()
    RETURNS TRIGGER AS $entries_statistics$
BEGIN
    IF (OLD.deleted = FALSE AND NEW.deleted = TRUE) THEN
        IF (NEW.role = 'ca4e3a74-86bb-4c6e-84b5-9e2da47d1b2e') THEN
            UPDATE entries_statistics
            SET managers_count = managers_count - 1
            WHERE instance_id = NEW.instance_id;
        ELSIF (NEW.role = 'aa522dd3-3a11-46a0-afa7-260b70609524') THEN
            UPDATE entries_statistics
            SET performers_count = performers_count - 1
            WHERE instance_id = NEW.instance_id;
        END IF;
    END IF;
    RETURN NEW;
END;
$entries_statistics$
LANGUAGE plpgsql;

CREATE TRIGGER insert_users_statistics_trigger
AFTER INSERT ON users FOR EACH ROW EXECUTE PROCEDURE insert_users_statistics();

CREATE TRIGGER delete_users_statistics_trigger
AFTER UPDATE ON users FOR EACH ROW EXECUTE PROCEDURE delete_users_statistics();


CREATE OR REPLACE FUNCTION insert_users_search()
    RETURNS TRIGGER AS $users$
BEGIN
    NEW.search := coalesce(NEW.code, '')
                  || ' '
                  || coalesce(NEW.surname, '')
                  || ' '
                  || coalesce(NEW.name, '')
                  || ' '
                  || coalesce(NEW.patronymic, '')
                  || ' '
                  || coalesce(NEW.phone_number, '');
    RETURN NEW;
END;
$users$
LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_users_search()
    RETURNS TRIGGER AS $users$
BEGIN
    NEW.search := coalesce(NEW.code, OLD.code, '')
                  || ' '
                  || coalesce(NEW.surname, OLD.surname, '')
                  || ' '
                  || coalesce(NEW.name, OLD.name, '')
                  || ' '
                  || coalesce(NEW.patronymic, OLD.patronymic, '')
                  || ' '
                  || coalesce(NEW.phone_number, OLD.phone_number, '');
    RETURN NEW;
END;
$users$
LANGUAGE plpgsql;

CREATE TRIGGER insert_users_search_trigger
BEFORE INSERT ON users FOR EACH ROW EXECUTE PROCEDURE insert_users_search();

CREATE TRIGGER update_users_search_trigger
BEFORE UPDATE ON users FOR EACH ROW EXECUTE PROCEDURE update_users_search();