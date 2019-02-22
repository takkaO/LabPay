#include <stdio.h>
#include <sqlite3.h>
#include <stdlib.h>
#include "db_ctrl.h"

int main(){
	int money = 0;
	GetMoneyValue("test_pw", &money);
}

int main_test(){
	printf("sqlite3 version = %s\n", sqlite3_version);
	int ret;
	sqlite3 *conn = NULL;
	sqlite3_stmt *statement = NULL;
	char *err_msg = NULL;
	char sql_str[255] = {0};

	// アクセス
	ret = sqlite3_open("db_test.sqlite3", &conn);
	if(ret != SQLITE_OK){
		puts("open error");
		exit(1);
	}

	ret = sqlite3_prepare_v2(conn, "UPDATE users SET money=97 WHERE password='test_pw'", -1, &statement, NULL);
	if (ret != SQLITE_OK){
		puts("process error");
		exit(1);
	}
	sqlite3_step(statement) ;
	/*while (sqlite3_step(statement) == SQLITE_ROW)
	{
		int money = sqlite3_column_int(statement, 0);
		const unsigned char *mail = sqlite3_column_text(statement, 1);
		const unsigned char *pw = sqlite3_column_text(statement, 2);
		printf("%d %s %s\n", money, mail, pw);
	}*/

	// クローズ
	sqlite3_finalize(statement);
	ret = sqlite3_close(conn);
	if(ret != SQLITE_OK){
		puts("close error");
		exit(1);
	}
	
}