CREATE TABLE public.invitations
(
    id UUID PRIMARY KEY NOT NULL,
    deleted BOOLEAN NOT NULL DEFAULT FALSE,
    invitation_code VARCHAR(30) NOT NULL,
    creator_id UUID NOT NULL,
    created TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    expires TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    instance_id UUID NOT NULL,
    role UUID NOT NULL,
    surname CITEXT,
    name CITEXT,
    patronymic CITEXT,
    phone_number VARCHAR(20),
    search CITEXT,
    CONSTRAINT invitations_users_id_fk FOREIGN KEY (creator_id) REFERENCES users (id),
    CONSTRAINT invitations_instances_id_fk FOREIGN KEY (instance_id) REFERENCES instances (id),
    CONSTRAINT users_roles_role_fk FOREIGN KEY (role) REFERENCES roles (id)
);
CREATE UNIQUE INDEX invitations_invitation_code_uindex ON public.invitations (invitation_code);


CREATE OR REPLACE FUNCTION insert_invitations_statistics()
    RETURNS TRIGGER AS $entries_statistics$
BEGIN
    IF (NEW.role = 'ca4e3a74-86bb-4c6e-84b5-9e2da47d1b2e')
    THEN
        UPDATE entries_statistics
        SET managers_invitations_count = managers_invitations_count + 1
        WHERE instance_id = NEW.instance_id;
    ELSIF (NEW.role = 'aa522dd3-3a11-46a0-afa7-260b70609524')
    THEN
        UPDATE entries_statistics
        SET performers_invitations_count = performers_invitations_count + 1
        WHERE instance_id = NEW.instance_id;
    END IF;
    RETURN NEW;
END;
$entries_statistics$
LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION delete_invitations_statistics()
    RETURNS TRIGGER AS $entries_statistics$
BEGIN
    IF (OLD.deleted = FALSE AND NEW.deleted = TRUE) THEN
        IF (NEW.role = 'ca4e3a74-86bb-4c6e-84b5-9e2da47d1b2e') THEN
            UPDATE entries_statistics
            SET managers_invitations_count = managers_invitations_count - 1
            WHERE instance_id = NEW.instance_id;
        ELSIF (NEW.role = 'aa522dd3-3a11-46a0-afa7-260b70609524') THEN
            UPDATE entries_statistics
            SET performers_invitations_count = performers_invitations_count - 1
            WHERE instance_id = NEW.instance_id;
        END IF;
    END IF;
    RETURN NEW;
END;
$entries_statistics$
LANGUAGE plpgsql;

CREATE TRIGGER insert_invitations_statistics_trigger
AFTER INSERT ON invitations FOR EACH ROW EXECUTE PROCEDURE insert_invitations_statistics();

CREATE TRIGGER delete_invitations_statistics_trigger
AFTER UPDATE ON invitations FOR EACH ROW EXECUTE PROCEDURE delete_invitations_statistics();


CREATE OR REPLACE FUNCTION insert_invitations_search()
    RETURNS TRIGGER AS $invitations$
BEGIN
    NEW.search := coalesce(NEW.invitation_code, '')
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
$invitations$
LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_invitations_search()
    RETURNS TRIGGER AS $invitations$
BEGIN
    NEW.search := coalesce(NEW.invitation_code, OLD.invitation_code, '')
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
$invitations$
LANGUAGE plpgsql;

CREATE TRIGGER insert_invitations_search_trigger
BEFORE INSERT ON invitations FOR EACH ROW EXECUTE PROCEDURE insert_invitations_search();

CREATE TRIGGER update_invitations_search_trigger
BEFORE UPDATE ON invitations FOR EACH ROW EXECUTE PROCEDURE update_invitations_search();