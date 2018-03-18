# pip3 install psycopg2

import psycopg2

if __name__ == '__main__':
    with open('connection_string', 'r') as connection_string_file:
        connection_string = connection_string_file.read()

    conn = psycopg2.connect(connection_string)
    cur = conn.cursor()

    cur.execute("""drop schema if exists "public" cascade;""")
    conn.commit()
    cur.close()
    conn.close()
