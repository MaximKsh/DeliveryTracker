-- Хорошо проанализировать запросы и обдумать денормализацию

CREATE TABLE public.tasks
(
    id UUID PRIMARY KEY NOT NULL,
    instance_id UUID NOT NULL,
    state_id UUID NOT NULL,
    author_id UUID NOT NULL,
    performer_id UUID,
    task_number CITEXT NOT NULL,
    created TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT ( now() AT TIME ZONE 'UTC'),
    state_changed_last_time TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT ( now() AT TIME ZONE 'UTC'),
    receipt TIMESTAMP WITHOUT TIME ZONE,
    receipt_actual TIMESTAMP WITHOUT TIME ZONE,
    delivery_from TIMESTAMP WITHOUT TIME ZONE,
    delivery_to TIMESTAMP WITHOUT TIME ZONE,
    delivery_eta TIMESTAMP WITHOUT TIME ZONE,
    delivery_actual TIMESTAMP WITHOUT TIME ZONE,
    comment CITEXT,
    warehouse_id UUID,
    client_id UUID,
    client_address_id UUID,
    payment_type_id UUID,
    cost DECIMAL,
    delivery_cost DECIMAL,
    CONSTRAINT tasks_instances_id_fk FOREIGN KEY (instance_id) REFERENCES instances (id),
    CONSTRAINT tasks_task_states_id_fk FOREIGN KEY (state_id) REFERENCES task_states (id),
    CONSTRAINT tasks_users_id_author_fk FOREIGN KEY (author_id) REFERENCES users (id),
    CONSTRAINT tasks_users_id_performer_fk FOREIGN KEY (performer_id) REFERENCES users (id),
    CONSTRAINT tasks_warehouses_id_fk FOREIGN KEY (warehouse_id) REFERENCES warehouses (id),
    CONSTRAINT tasks_clients_id_fk FOREIGN KEY (client_id) REFERENCES clients (id),
    CONSTRAINT tasks_client_addresses_id_fk FOREIGN KEY (client_address_id) REFERENCES client_addresses (id),
    CONSTRAINT tasks_payment_types_id_fk FOREIGN KEY (payment_type_id) REFERENCES payment_types (id)
);

CREATE INDEX tasks_instance_id_index ON public.tasks (instance_id);
CREATE INDEX tasks_author_id_index ON public.tasks (author_id);
CREATE INDEX tasks_performer_id_index ON public.tasks (performer_id);
CREATE INDEX tasks_state_changed_last_time_index ON public.tasks (state_changed_last_time);
CREATE INDEX tasks_delivery_from_state_id_index ON public.tasks (cast(delivery_from as date), state_id);