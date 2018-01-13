CREATE TABLE public.instances
(
    id UUID PRIMARY KEY,
    name CITEXT NOT NULL,
    creator_id UUID NOT NULL
);
