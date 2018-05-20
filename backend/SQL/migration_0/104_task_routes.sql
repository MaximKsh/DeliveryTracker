CREATE TABLE public.task_routes
(
    id uuid PRIMARY KEY NOT NULL,
    instance_id uuid NOT NULL,
    task_id uuid NOT NULL,
    performer_id uuid NOT NULL,
    eta_offset int NOT NULL,
    date date NOT NULL,
    CONSTRAINT task_routes_instance_id_fk FOREIGN KEY (instance_id) REFERENCES instances (id),
    CONSTRAINT task_routes_users_id_fk FOREIGN KEY (performer_id) REFERENCES users (id),
    CONSTRAINT task_routes_tasks_id_fk FOREIGN KEY (task_id) REFERENCES tasks (id)
);
CREATE INDEX task_routes_task_id_index ON public.task_routes (task_id);
CREATE INDEX task_routes_date_instance_id_performer_id_eta_offset_index
    ON public.task_routes (date, instance_id, performer_id, eta_offset);