CREATE TABLE public.manager_statistics
(
    id uuid PRIMARY KEY,
    user_id uuid NOT NULL,
    date timestamp without time zone NOT NULL,
    created_tasks int DEFAULT 0 NOT NULL,
    completed_tasks int DEFAULT 0 NOT NULL,
    
    CONSTRAINT manager_statistics_users_id_fk FOREIGN KEY (user_id) REFERENCES users (id)
);
CREATE UNIQUE INDEX manager_statistics_user_id_date_uindex ON public.manager_statistics (user_id, date ASC);