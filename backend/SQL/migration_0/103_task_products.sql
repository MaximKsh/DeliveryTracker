CREATE TABLE public.task_products
(
    id UUID PRIMARY KEY NOT NULL,
    task_id UUID NOT NULL,
    product_id UUID NOT NULL,
    quantity INT DEFAULT 1 NOT NULL,
    CONSTRAINT task_products_tasks_id_fk FOREIGN KEY (task_id) REFERENCES tasks (id)
);
CREATE INDEX task_products_task_id_index ON public.task_products (task_id);