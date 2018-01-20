# pip3 install psycopg2

import psycopg2
import hashlib
from os import listdir, getcwd
from os.path import isfile, join, isdir
import re

def get_migration_history(conn, cur):
    try:
        cur.execute("""
            SELECT 
              id,
              directory,
              script_hash,
              executed_time
            FROM migrations
            ORDER BY directory;
        """)
        migrations = []
        for row in cur:
            migrations.append({
                'id': row[0],
                'directory': row[1],
                'script_hash': row[2],
                'executed_time': row[3]
            })
        return migrations
    except psycopg2.ProgrammingError as e:
        conn.reset()
        if e.pgcode == '42P01' and e.pgerror.startswith('ERROR:  relation "migrations" does not exist'):
            create_migration_history(cur)
            return []
        raise


def create_migration_history(cur):
    cur.execute("""
        CREATE TABLE migrations(
            id UUID PRIMARY KEY,
            directory TEXT NOT NULL,
            script_hash TEXT NOT NULL,
            executed_time TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT(now() AT TIME ZONE 'UTC')
        );   
        CREATE UNIQUE INDEX migrations_directory_uindex ON public.migrations (directory);    
    """)


def get_sql_from_directory(directory):
    files = list([f for f in listdir(directory) if isfile(join(directory, f)) and f.endswith('.sql')])
    files.sort(key=first_int_from_str)
    file_contents = []
    for filename in files:
        with open(join(directory, filename), 'r') as file:
            file_contents.append('-- ' + filename + '\n' + file.read() + '\n\n')
    return ''.join(file_contents)

def first_int_from_str(string_with_int):
    values = list(map(int, re.findall('\d+', string_with_int)))
    if len(values) == 0:
        return -1
    else:
        return values[0]

def get_hash(script):
    return hashlib.md5(script.encode()).hexdigest()


def cancel(ret_val, cur, conn, text):
    print(text)
    cur.close()
    conn.close()
    exit(ret_val)


def insert_migration(cur, directory, hash):
    cur.execute("""
        INSERT INTO migrations(id, directory, script_hash)
        VALUES (uuid_generate_v4(), %s, %s) ;   
    """, (directory, hash))


if __name__ == '__main__':
    with open('connection_string', 'r') as connection_string_file:
        connection_string = connection_string_file.read()

    conn = psycopg2.connect(connection_string)
    cur = conn.cursor()
    cur_dir = getcwd()

    migrations = get_migration_history(conn, cur)
    directories = list([f for f in listdir(cur_dir) if isdir(join(cur_dir, f)) and f.startswith('migration_')])
    directories.sort(key=first_int_from_str)

    dir_index = 0
    for migration in migrations:
        print('Проверка миграции')
        print(migration)
        directory = directories[dir_index]
        if directory != migration['directory']:
            cancel(1, cur, conn,
                   'Миграция ' + migration['directory'] + ' под номером '
                   + str(dir_index) + ' не соответствует миграции на диске ' + directory + '\nОтмена.')
        if get_hash(get_sql_from_directory(directory)) != migration['script_hash']:
            cancel(1, cur, conn,
                   'Хэш миграции ' + migration['directory'] + ' под номером ' + str(dir_index)
                   + ' не соответствует содержимому миграции на диске ' + directory + '\nОтмена.')
        dir_index += 1

    dir_size = len(directories)
    if dir_size == dir_index:
        cancel(0, cur, conn, 'Нет новых миграций')

    while dir_index != dir_size:
        directory = directories[dir_index]
        script = get_sql_from_directory(directory)
        print(directory)
        print('Выполняется запрос: ')
        print(script)
        cur.execute(script)
        insert_migration(cur, directory, get_hash(script))
        dir_index += 1

    script = get_sql_from_directory(directories[0])
    conn.commit()
    cur.close()
    conn.close()
