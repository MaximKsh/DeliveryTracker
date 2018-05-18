CREATE TABLE public.roles
(
    id UUID PRIMARY KEY NOT NULL,
    name VARCHAR(50) NOT NULL,
    caption CITEXT NOT NULL
);

INSERT INTO public.roles(id, name, caption)
VALUES ('fbe65847-57c0-42a9-84a9-3f95b92fd39e', 'CreatorRole', 'ServerMessage_Roles_CreatorRole'),
       ('ca4e3a74-86bb-4c6e-84b5-9e2da47d1b2e', 'ManagerRole', 'ServerMessage_Roles_ManagerRole'),
       ('aa522dd3-3a11-46a0-afa7-260b70609524', 'PerformerRole', 'ServerMessage_Roles_PerformerRole'),
       ('cc3c251c-dfa1-4ff3-8116-8f24aa8176a5', 'SchedulerRole', 'ServerMessage_Roles_SchedulerRole');