CREATE TABLE public.task_products
(
    id UUID PRIMARY KEY NOT NULL,
    instance_id UUID NOT NULL,
    parent_id UUID NOT NULL,
    product_id UUID NOT NULL,
    quantity INT DEFAULT 1 NOT NULL,
    CONSTRAINT task_products_tasks_id_fk FOREIGN KEY (parent_id) REFERENCES tasks (id),
    CONSTRAINT task_products_products_id_fk FOREIGN KEY (product_id) REFERENCES products (id)
);
CREATE UNIQUE INDEX task_products_task_id_product_id_index ON public.task_products (parent_id, product_id);