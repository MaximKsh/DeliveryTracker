CREATE TABLE public.task_states
(
    id UUID PRIMARY KEY NOT NULL,
    name VARCHAR(50) NOT NULL,
    caption CITEXT NOT NULL
);

INSERT INTO task_states(id, name, caption)
VALUES ('3f7d33e6-5ecf-41a3-876a-c0345f690ca4', 'Unconfirmed', 'ServerMessage_TaskStates_Unconfirmed'),
       ('135374ad-ba12-4ada-9dc7-8f8e3b11d9e5', 'New', 'ServerMessage_TaskStates_New'),
       ('beaff26f-2193-41ed-b2a6-983b707a216d', 'InProgress', 'ServerMessage_TaskStates_InProgress'),
       ('d91856f9-d1bf-4fad-a46e-c3baafabf762', 'Complete', 'ServerMessage_TaskStates_Complete'),
       ('1483e2f3-5bcf-48ca-bcaa-870573997465', 'Cancelled', 'ServerMessage_TaskStates_Cancelled')

;

