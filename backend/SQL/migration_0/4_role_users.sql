CREATE TABLE public.role_users
(
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL,
    role_id UUID NOT NULL,
    CONSTRAINT role_users_users_id_fk FOREIGN KEY (user_id) REFERENCES users (id),
    CONSTRAINT role_users_roles_id_fk FOREIGN KEY (role_id) REFERENCES roles (id)
);
CREATE UNIQUE INDEX role_users_user_id_role_id_uindex ON public.role_users (user_id, role_id);
CREATE INDEX role_users_role_id_index ON public.role_users (role_id);