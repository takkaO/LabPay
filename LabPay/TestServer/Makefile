sqlite: sqlite.c
	gcc sqlite.c -o sqlite -lsqlite3

server: server.c hoge.c
	gcc server.c hoge.c -o server

test: test.c
	gcc test.c -o test -lcrypt

all: sqlite.c server.c
	gcc sqlite.c -o sqlite -lsqlite3
	gcc server.c -o server
	gcc test.c -o test -lcrypt