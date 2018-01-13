CREATE TABLE public.role_users
(
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL,
    role_id UUID NOT NULL,
    CONSTRAINT role_users_users_id_fk FOREIGN KEY (user_id) REFERENCES users (id),
    CONSTRAINT role_users_roles_id_fk FOREIGN KEY (role_id) REFERENCES roles (id)
);
