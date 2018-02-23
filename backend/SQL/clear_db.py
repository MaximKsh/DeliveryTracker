# pip3 install psycopg2

import psycopg2



if __name__ == '__main__':
    with open('connection_string', 'r') as connection_string_file:
        connection_string = connection_string_file.read()

    conn = psycopg2.connect(connection_string)
    cur = conn.cursor()

    cur.execute("""select extname from pg_extension where extname <> 'plpgsql';""")
    queries = []
    for row in cur:
        queries.append('drop extension if exists "' + row[0] + '" cascade;\n')
    full_query = ''.join(queries)
    print(full_query)
    cur.execute(full_query)

    cur.execute(""" select tablename from pg_tables where "schemaname" = 'public';""")
    queries = []
    for row in cur:
        queries.append('drop table if exists ' + row[0] + ' cascade;\n')
    full_query = ''.join(queries)
    print(full_query)
    cur.execute(full_query)
     
    conn.commit()
    cur.close()
    conn.close()
