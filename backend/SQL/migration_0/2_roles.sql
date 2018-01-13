
CREATE TABLE public.roles
(
    id UUID PRIMARY KEY,
    name CITEXT NOT NULL
);
CREATE UNIQUE INDEX roles_name_uindex ON public.roles (name);

INSERT INTO roles (id, "name")
VALUES
  ('c39e67b9-5f70-47f3-861b-2566f42123b9', 'ServerMessage_Roles_CreatorRole'),
  ('5c797067-ebd1-4a62-a635-b8493713c136', 'ServerMessage_Roles_ManagerRole'),
  ('410bca25-f2fc-4de5-be75-c1dd42b60582', 'ServerMessage_Roles_PerformerRole')
;
