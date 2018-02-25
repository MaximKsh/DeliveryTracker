CREATE TABLE public.task_state_transitions
(
    id UUID PRIMARY KEY NOT NULL,
    role UUID NOT NULL,
    initial_state UUID NOT NULL,
    final_state UUID NOT NULL,
    button_caption CITEXT NOT NULL,
    CONSTRAINT task_state_transitions_roles_id_fk FOREIGN KEY (role) REFERENCES roles (id),
    CONSTRAINT task_state_transitions_task_states_id_initial_fk FOREIGN KEY (initial_state) REFERENCES task_states (id),
    CONSTRAINT task_state_transitions_task_states_id_final_fk FOREIGN KEY (final_state) REFERENCES task_states (id)
);
CREATE UNIQUE INDEX task_state_transitions_role_initial_state_final_state_uindex ON public.task_state_transitions (role, initial_state, final_state);


INSERT INTO task_state_transitions(id, role, initial_state, final_state, button_caption)
        -- Manager: Unconfirmed -> New
VALUES ('f81c7dd5-57e3-4bd6-ae10-60ef99a232bd', 'ca4e3a74-86bb-4c6e-84b5-9e2da47d1b2e', '3f7d33e6-5ecf-41a3-876a-c0345f690ca4', '135374ad-ba12-4ada-9dc7-8f8e3b11d9e5', 'ServerMessage_StateTransitionButton_ManagerUnconfirmedNew')


;