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

-- Preparing  = 8c9c1011-f7c1-4cef-902f-4925f5e83f4a
-- Queue      = d4595da3-6a5f-4455-b975-7637ea429cb5
-- Waiting    = 0a79703f-4570-4a58-8509-9e598b1eefaf
-- IntoWork   = 8912d18f-192a-4327-bd47-5c9963b5f2b0
-- Delivered  = 020d7c7e-bb4e-4add-8b11-62a91471b7c8
-- Complete   = d91856f9-d1bf-4fad-a46e-c3baafabf762
-- Revoked    = d2e70369-3d37-420f-b176-5fa0b2c1d4a9

-- Creator    = fbe65847-57c0-42a9-84a9-3f95b92fd39e
-- Manager    = ca4e3a74-86bb-4c6e-84b5-9e2da47d1b2e
-- Performer  = aa522dd3-3a11-46a0-afa7-260b70609524

-- Creator template
-- -- Creator:  ->
-- ('', 'fbe65847-57c0-42a9-84a9-3f95b92fd39e', '', '', 'ServerMessage_TransitionButton_Creator'),

-- Manager template
-- -- Manager:  ->
-- ('', 'ca4e3a74-86bb-4c6e-84b5-9e2da47d1b2e', '', '', 'ServerMessage_TransitionButton_Manager'),

-- Performer template
-- -- Performer:  ->
-- ('', 'aa522dd3-3a11-46a0-afa7-260b70609524 ', '', '', 'ServerMessage_TransitionButton_Performer'),

INSERT INTO task_state_transitions(id, role, initial_state, final_state, button_caption)
     VALUES
-- Creator: Preparing -> Queue
('7264608a-bb5c-4b26-9593-4d55547a1586', 'fbe65847-57c0-42a9-84a9-3f95b92fd39e', '8c9c1011-f7c1-4cef-902f-4925f5e83f4a', 'd4595da3-6a5f-4455-b975-7637ea429cb5', 'ServerMessage_TransitionButton_CreatorPreparingQueue'),
-- Creator: Queue -> Preparing
('69c7a6e0-fa0a-4acc-978c-fd68b08eac09', 'fbe65847-57c0-42a9-84a9-3f95b92fd39e', 'd4595da3-6a5f-4455-b975-7637ea429cb5', '8c9c1011-f7c1-4cef-902f-4925f5e83f4a', 'ServerMessage_TransitionButton_CreatorQueuePreparing'),
-- Creator: Queue -> Waiting
('21fe3ed6-ce75-4a9c-af1e-281736504bb9', 'fbe65847-57c0-42a9-84a9-3f95b92fd39e', 'd4595da3-6a5f-4455-b975-7637ea429cb5', '0a79703f-4570-4a58-8509-9e598b1eefaf', 'ServerMessage_TransitionButton_CreatorQueueWaiting'),
-- Creator: Waiting -> Queue
('5831fa1d-648d-4647-a461-427a61ab0d1a', 'fbe65847-57c0-42a9-84a9-3f95b92fd39e', '0a79703f-4570-4a58-8509-9e598b1eefaf', 'd4595da3-6a5f-4455-b975-7637ea429cb5', 'ServerMessage_TransitionButton_CreatorWaitingQueue'),
-- Creator: Waiting -> Preparing
('9724a247-2320-4a83-be61-4e113da7c470', 'fbe65847-57c0-42a9-84a9-3f95b92fd39e', '0a79703f-4570-4a58-8509-9e598b1eefaf', '8c9c1011-f7c1-4cef-902f-4925f5e83f4a', 'ServerMessage_TransitionButton_CreatorWaitingPreparing'),
-- Creator: Delivered -> Complete
('3df4f21f-f566-4f29-ae38-90f503ceccbd', 'fbe65847-57c0-42a9-84a9-3f95b92fd39e', '020d7c7e-bb4e-4add-8b11-62a91471b7c8', 'd91856f9-d1bf-4fad-a46e-c3baafabf762', 'ServerMessage_TransitionButton_CreatorDeliveredComplete'),
-- Creator: Delivered -> Preparing
('5d15f1a4-729c-4061-9387-c2f4d8872bfc', 'fbe65847-57c0-42a9-84a9-3f95b92fd39e', '020d7c7e-bb4e-4add-8b11-62a91471b7c8', '8c9c1011-f7c1-4cef-902f-4925f5e83f4a', 'ServerMessage_TransitionButton_CreatorDeliveredPreparing'),
-- Creator: Preparing -> Revoked
('b087f9a5-3505-4833-9d9c-874ab2697531', 'fbe65847-57c0-42a9-84a9-3f95b92fd39e', '8c9c1011-f7c1-4cef-902f-4925f5e83f4a', 'd2e70369-3d37-420f-b176-5fa0b2c1d4a9', 'ServerMessage_TransitionButton_CreatorPreparingRevoked'),
-- Creator: Queue -> Revoked
('dbc17146-c414-4130-96d5-893a32208ce9', 'fbe65847-57c0-42a9-84a9-3f95b92fd39e', 'd4595da3-6a5f-4455-b975-7637ea429cb5', 'd2e70369-3d37-420f-b176-5fa0b2c1d4a9', 'ServerMessage_TransitionButton_CreatorQueueRevoked'),
-- Creator: Waiting -> Revoked
('231de99d-bdf2-4c68-bd7b-359001444a5f', 'fbe65847-57c0-42a9-84a9-3f95b92fd39e', '0a79703f-4570-4a58-8509-9e598b1eefaf', 'd2e70369-3d37-420f-b176-5fa0b2c1d4a9', 'ServerMessage_TransitionButton_CreatorWaitingRevoked'),
-- Creator: IntoWork -> Revoked
('099fdebd-333f-4030-b092-752555564c9c', 'fbe65847-57c0-42a9-84a9-3f95b92fd39e', '8912d18f-192a-4327-bd47-5c9963b5f2b0', 'd2e70369-3d37-420f-b176-5fa0b2c1d4a9', 'ServerMessage_TransitionButton_CreatorIntoWorkRevoked'),
         
         
         
-- Manager: Preparing -> Queue
('51cc819f-2d55-44da-86d2-1480c8ec55c3', 'ca4e3a74-86bb-4c6e-84b5-9e2da47d1b2e', '8c9c1011-f7c1-4cef-902f-4925f5e83f4a', 'd4595da3-6a5f-4455-b975-7637ea429cb5', 'ServerMessage_TransitionButton_ManagerPreparingQueue'),
-- Manager: Queue -> Preparing
('2a7a6ffe-dc47-4510-8e85-9e629cb21b21', 'ca4e3a74-86bb-4c6e-84b5-9e2da47d1b2e', 'd4595da3-6a5f-4455-b975-7637ea429cb5', '8c9c1011-f7c1-4cef-902f-4925f5e83f4a', 'ServerMessage_TransitionButton_ManagerQueuePreparing'),
-- Manager: Queue -> Waiting
('90f199fe-eb59-4b6e-8e50-405a0c60a73c', 'ca4e3a74-86bb-4c6e-84b5-9e2da47d1b2e', 'd4595da3-6a5f-4455-b975-7637ea429cb5', '0a79703f-4570-4a58-8509-9e598b1eefaf', 'ServerMessage_TransitionButton_ManagerQueueWaiting'),
-- Manager: Waiting -> Queue
('4790ff27-4688-4d31-9f71-bf2612fddc06', 'ca4e3a74-86bb-4c6e-84b5-9e2da47d1b2e', '0a79703f-4570-4a58-8509-9e598b1eefaf', 'd4595da3-6a5f-4455-b975-7637ea429cb5', 'ServerMessage_TransitionButton_ManagerWaitingQueue'),
-- Manager: Waiting -> Preparing
('86fa178f-59a5-4922-b8fc-c0b0ead73c89', 'ca4e3a74-86bb-4c6e-84b5-9e2da47d1b2e', '0a79703f-4570-4a58-8509-9e598b1eefaf', '8c9c1011-f7c1-4cef-902f-4925f5e83f4a', 'ServerMessage_TransitionButton_ManagerWaitingPreparing'),
-- Manager: Delivered -> Complete
('95634e57-3774-4358-8ac8-c38236fb184d', 'ca4e3a74-86bb-4c6e-84b5-9e2da47d1b2e', '020d7c7e-bb4e-4add-8b11-62a91471b7c8', 'd91856f9-d1bf-4fad-a46e-c3baafabf762', 'ServerMessage_TransitionButton_ManagerDeliveredComplete'),
-- Manager: Delivered -> Preparing
('4677f1e2-aad1-488d-b5b2-12e61b3ae6af', 'ca4e3a74-86bb-4c6e-84b5-9e2da47d1b2e', '020d7c7e-bb4e-4add-8b11-62a91471b7c8', '8c9c1011-f7c1-4cef-902f-4925f5e83f4a', 'ServerMessage_TransitionButton_ManagerDeliveredPreparing'),
-- Manager: Preparing -> Revoked
('ed81fa81-143a-4c9d-8105-d8bf646228a3', 'ca4e3a74-86bb-4c6e-84b5-9e2da47d1b2e', '8c9c1011-f7c1-4cef-902f-4925f5e83f4a', 'd2e70369-3d37-420f-b176-5fa0b2c1d4a9', 'ServerMessage_TransitionButton_ManagerPreparingRevoked'),
-- Manager: Queue -> Revoked
('ff200cb3-8fbf-4d77-9f52-4748a851fda3', 'ca4e3a74-86bb-4c6e-84b5-9e2da47d1b2e', 'd4595da3-6a5f-4455-b975-7637ea429cb5', 'd2e70369-3d37-420f-b176-5fa0b2c1d4a9', 'ServerMessage_TransitionButton_ManagerQueueRevoked'),
-- Manager: Waiting -> Revoked
('5c8b1f45-0660-41e4-99eb-bd3203b4edda', 'ca4e3a74-86bb-4c6e-84b5-9e2da47d1b2e', '0a79703f-4570-4a58-8509-9e598b1eefaf', 'd2e70369-3d37-420f-b176-5fa0b2c1d4a9', 'ServerMessage_TransitionButton_ManagerWaitingRevoked'),
-- Manager: IntoWork -> Revoked
('6c93b474-535c-4a06-bb58-39badc79fe84', 'ca4e3a74-86bb-4c6e-84b5-9e2da47d1b2e', '8912d18f-192a-4327-bd47-5c9963b5f2b0', 'd2e70369-3d37-420f-b176-5fa0b2c1d4a9', 'ServerMessage_TransitionButton_ManagerIntoWorkRevoked'),




-- Performer: Waiting -> IntoWork
('b16b49a2-927b-4db4-9327-20e9d632c7b0', 'aa522dd3-3a11-46a0-afa7-260b70609524', '0a79703f-4570-4a58-8509-9e598b1eefaf', '8912d18f-192a-4327-bd47-5c9963b5f2b0', 'ServerMessage_TransitionButton_PerformerWaitingIntoWork'),
-- Performer: IntoWork -> Waiting
('4b70c6ef-59a5-48a2-aaaa-e5fa6a7e0723', 'aa522dd3-3a11-46a0-afa7-260b70609524', '8912d18f-192a-4327-bd47-5c9963b5f2b0', '0a79703f-4570-4a58-8509-9e598b1eefaf', 'ServerMessage_TransitionButton_PerformerIntoWorkWaiting'),
-- Performer: IntoWork -> Delivered
('bff72a68-ad78-42a7-b28b-50f79302fdf9', 'aa522dd3-3a11-46a0-afa7-260b70609524', '8912d18f-192a-4327-bd47-5c9963b5f2b0', '020d7c7e-bb4e-4add-8b11-62a91471b7c8', 'ServerMessage_TransitionButton_PerformerIntoWorkDelivered')

;