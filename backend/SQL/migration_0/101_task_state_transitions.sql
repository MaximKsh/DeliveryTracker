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
    VALUES
-- Manager: Unconfirmed -> New
('f81c7dd5-57e3-4bd6-ae10-60ef99a232bd', 'ca4e3a74-86bb-4c6e-84b5-9e2da47d1b2e', '3f7d33e6-5ecf-41a3-876a-c0345f690ca4', '135374ad-ba12-4ada-9dc7-8f8e3b11d9e5', 'ServerMessage_StateTransitionButton_ManagerUnconfirmedNew'),
-- Manager: New -> InProgress
('13c933dd-ab35-4175-be2d-69996bec55c2', 'ca4e3a74-86bb-4c6e-84b5-9e2da47d1b2e', '135374ad-ba12-4ada-9dc7-8f8e3b11d9e5', 'beaff26f-2193-41ed-b2a6-983b707a216d', 'ServerMessage_StateTransitionButton_ManagerNewInProgress'),
-- Manager: InProgress -> Complete
('35cccef0-35cf-4726-8adb-ffc32079b08d', 'ca4e3a74-86bb-4c6e-84b5-9e2da47d1b2e', 'beaff26f-2193-41ed-b2a6-983b707a216d', 'd91856f9-d1bf-4fad-a46e-c3baafabf762', 'ServerMessage_StateTransitionButton_ManagerInProgressComplete'),
-- Manager: InProgress -> Cancel
('1e939c10-90d9-4715-8d95-cf9a96da280d', 'ca4e3a74-86bb-4c6e-84b5-9e2da47d1b2e', 'beaff26f-2193-41ed-b2a6-983b707a216d', '1483e2f3-5bcf-48ca-bcaa-870573997465', 'ServerMessage_StateTransitionButton_ManagerInProgressCancelled'),

-- Creator: Unconfirmed -> New
('47569906-91f5-4944-8e9d-77e503025ad4', 'fbe65847-57c0-42a9-84a9-3f95b92fd39e', '3f7d33e6-5ecf-41a3-876a-c0345f690ca4', '135374ad-ba12-4ada-9dc7-8f8e3b11d9e5', 'ServerMessage_StateTransitionButton_CreatorUnconfirmedNew'),
-- Creator: New -> InProgress
('3300a79b-88fb-4ba9-90c7-42476e623c59', 'fbe65847-57c0-42a9-84a9-3f95b92fd39e', '135374ad-ba12-4ada-9dc7-8f8e3b11d9e5', 'beaff26f-2193-41ed-b2a6-983b707a216d', 'ServerMessage_StateTransitionButton_CreatorNewInProgress'),
-- Creator: InProgress -> Complete
('88922080-00bd-4bf7-99d6-45acf463d77b', 'fbe65847-57c0-42a9-84a9-3f95b92fd39e', 'beaff26f-2193-41ed-b2a6-983b707a216d', 'd91856f9-d1bf-4fad-a46e-c3baafabf762', 'ServerMessage_StateTransitionButton_CreatorInProgressComplete'),
-- Creator: InProgress -> Cancel
('0049d17b-d3d9-4c56-b05f-a21c6f1658e3', 'fbe65847-57c0-42a9-84a9-3f95b92fd39e', 'beaff26f-2193-41ed-b2a6-983b707a216d', '1483e2f3-5bcf-48ca-bcaa-870573997465', 'ServerMessage_StateTransitionButton_CreatorInProgressCancelled')



;