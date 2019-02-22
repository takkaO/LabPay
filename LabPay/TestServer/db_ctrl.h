#ifndef DB_CTRL_H
#define DB_CTRL_H

#include <stdio.h>
#include <sqlite3.h>
#include <stdlib.h>
#include <string.h>

#define TRUE 1
#define FALSE 0

enum DBError
{
	DB_OPEN_ERROR,
	DB_PROCESS_ERROR,
	DB_HASH_CONFLICT,
	DB_CLOSE_ERROR,
	DB_QUERY_ERROR,
	DB_NO_ERROR
};

struct UserInfo
{
	char hash[65]; // 32byteを文字列00~FFに変換するので64byte+null
	char email[255];
	int money;
};

enum DBError UpdateMoneyValue(struct UserInfo user);
enum DBError GetMoneyValue(const char *hash, int *money);
enum DBError CheckHashConflict(const char *hash);
enum DBError InsertUserInfo(struct UserInfo user);

#endif