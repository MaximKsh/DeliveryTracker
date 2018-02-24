CREATE TABLE public.invitations
(
    id UUID PRIMARY KEY NOT NULL,
    invitation_code VARCHAR(20) NOT NULL,
    creator_id UUID NOT NULL,
    created TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    expires TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    instance_id UUID NOT NULL,
    role UUID NOT NULL,
    surname CITEXT,
    name CITEXT,
    patronymic CITEXT,
    phone_number VARCHAR(20),
    CONSTRAINT invitations_users_id_fk FOREIGN KEY (creator_id) REFERENCES users (id),
    CONSTRAINT invitations_instances_id_fk FOREIGN KEY (instance_id) REFERENCES instances (id),
    CONSTRAINT users_roles_role_fk FOREIGN KEY (role) REFERENCES roles (id)
);
CREATE UNIQUE INDEX invitations_invitation_code_uindex ON public.invitations (invitation_code);