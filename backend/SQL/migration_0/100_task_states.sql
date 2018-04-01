CREATE TABLE public.task_states
(
    id UUID PRIMARY KEY NOT NULL,
    name VARCHAR(50) NOT NULL,
    caption CITEXT NOT NULL
);

INSERT INTO task_states(id, name, caption)
VALUES
       ('8c9c1011-f7c1-4cef-902f-4925f5e83f4a', 'Preparing', 'ServerMessage_TaskStates_Preparing'),
       ('d4595da3-6a5f-4455-b975-7637ea429cb5', 'Queue', 'ServerMessage_TaskStates_Queue'),
       ('0a79703f-4570-4a58-8509-9e598b1eefaf', 'Waiting', 'ServerMessage_TaskStates_Waiting'),
       ('8912d18f-192a-4327-bd47-5c9963b5f2b0', 'IntoWork', 'ServerMessage_TaskStates_IntoWork'),
       ('020d7c7e-bb4e-4add-8b11-62a91471b7c8', 'Delivered', 'ServerMessage_TaskStates_Delivered'),
       ('d91856f9-d1bf-4fad-a46e-c3baafabf762', 'Complete', 'ServerMessage_TaskStates_Complete'),
       ('d2e70369-3d37-420f-b176-5fa0b2c1d4a9', 'Revoked', 'ServerMessage_TaskStates_Revoked')
;