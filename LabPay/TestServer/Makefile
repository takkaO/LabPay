sqlite: sqlite.c
	gcc sqlite.c -o sqlite -lsqlite3

server: server.c
	gcc server.c -o server

all: sqlite.c server.c
	gcc sqlite.c -o sqlite -lsqlite3
	gcc server.c -o server