CREATE TABLE public.entries_statistics
(
    instance_id UUID PRIMARY KEY NOT NULL,
    managers_count BIGINT DEFAULT 0 NOT NULL,
    performers_count BIGINT DEFAULT 0 NOT NULL,
    managers_invitations_count BIGINT DEFAULT 0 NOT NULL,
    performers_invitations_count BIGINT DEFAULT 0 NOT NULL,
    -- статистика по таскам здесь
    clients_count BIGINT DEFAULT 0 NOT NULL,
    payment_types_count BIGINT DEFAULT 0 NOT NULL,
    products_count BIGINT DEFAULT 0 NOT NULL,
    warehouses_count BIGINT DEFAULT 0 NOT NULL,
    CONSTRAINT entries_statistics_instances_id_fk FOREIGN KEY (instance_id) REFERENCES instances (id) ON DELETE CASCADE ON UPDATE CASCADE
);