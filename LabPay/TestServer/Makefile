sqlite: sqlite.c db_ctrl.c
	gcc sqlite.c db_ctrl.c -o sqlite -lsqlite3

server: server_main.c server.c db_ctrl.c
	gcc server_main.c server.c db_ctrl.c -o server -lsqlite3