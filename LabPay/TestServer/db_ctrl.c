#include "db_ctrl.h"

#define MAX_QUERY_LENGTH 2048

enum DBError UpdateMoneyValue(struct UserInfo user)
{
	int ret;
	int n;
	int conflict_flag = FALSE;
	sqlite3 *conn = NULL;
	sqlite3_stmt *statement = NULL;
	char query[MAX_QUERY_LENGTH] = {0};
	int money = 0;

	ret = GetMoneyValue(user.hash, &money);
	if (ret != DB_NO_ERROR)
	{
		return ret;
	}
	user.money += money;

	n = snprintf(query, MAX_QUERY_LENGTH, "UPDATE users SET money=%d WHERE password='%s'", user.money, user.hash);
	if (n < 0 || MAX_QUERY_LENGTH <= n)
	{
		return DB_QUERY_ERROR;
	}

	// アクセス
	ret = sqlite3_open("db_test.sqlite3", &conn);
	if (ret != SQLITE_OK)
	{
		return DB_OPEN_ERROR;
	}

	ret = sqlite3_prepare_v2(conn, query, -1, &statement, NULL);
	if (ret != SQLITE_OK)
	{
		return DB_PROCESS_ERROR;
	}
	sqlite3_step(statement);

	// クローズ
	sqlite3_finalize(statement);
	ret = sqlite3_close(conn);
	if (ret != SQLITE_OK)
	{
		return DB_CLOSE_ERROR;
	}
	if (conflict_flag == TRUE)
	{
		return DB_HASH_CONFLICT;
	}
	return DB_NO_ERROR;
}

enum DBError GetMoneyValue(const char *hash, int *money)
{
	int ret;
	int n;
	int conflict_flag = FALSE;
	sqlite3 *conn = NULL;
	sqlite3_stmt *statement = NULL;
	char query[MAX_QUERY_LENGTH] = {0};

	n = snprintf(query, MAX_QUERY_LENGTH, "SELECT money FROM users WHERE password='%s'", hash);
	if (n < 0 || MAX_QUERY_LENGTH <= n)
	{
		return DB_QUERY_ERROR;
	}

	// アクセス
	ret = sqlite3_open("db_test.sqlite3", &conn);
	if (ret != SQLITE_OK)
	{
		return DB_OPEN_ERROR;
	}

	ret = sqlite3_prepare_v2(conn, query, -1, &statement, NULL);
	if (ret != SQLITE_OK)
	{
		return DB_PROCESS_ERROR;
	}
	while (sqlite3_step(statement) == SQLITE_ROW)
	{
		*money = sqlite3_column_int(statement, 0);
	}

	// クローズ
	sqlite3_finalize(statement);
	ret = sqlite3_close(conn);
	if (ret != SQLITE_OK)
	{
		return DB_CLOSE_ERROR;
	}
	if (conflict_flag == TRUE)
	{
		return DB_HASH_CONFLICT;
	}
	return DB_NO_ERROR;
}

enum DBError CheckHashConflict(const char *hash)
{
	int ret;
	int n;
	int conflict_flag = FALSE;
	sqlite3 *conn = NULL;
	sqlite3_stmt *statement = NULL;
	char query[MAX_QUERY_LENGTH] = {0};

	n = snprintf(query, MAX_QUERY_LENGTH, "SELECT * FROM users");
	if (n < 0 || MAX_QUERY_LENGTH <= n)
	{
		return DB_QUERY_ERROR;
	}

	// アクセス
	ret = sqlite3_open("db_test.sqlite3", &conn);
	if (ret != SQLITE_OK)
	{
		return DB_OPEN_ERROR;
	}

	ret = sqlite3_prepare_v2(conn, query, -1, &statement, NULL);
	if (ret != SQLITE_OK)
	{
		return DB_PROCESS_ERROR;
	}
	while (sqlite3_step(statement) == SQLITE_ROW)
	{
		const unsigned char *pw_hash = sqlite3_column_text(statement, 2);
		if (strcmp(pw_hash, hash) == 0)
		{
			conflict_flag = TRUE;
			break;
		}
	}

	// クローズ
	sqlite3_finalize(statement);
	ret = sqlite3_close(conn);
	if (ret != SQLITE_OK)
	{
		return DB_CLOSE_ERROR;
	}
	if (conflict_flag == TRUE)
	{
		return DB_HASH_CONFLICT;
	}
	return DB_NO_ERROR;
}

enum DBError InsertUserInfo(struct UserInfo user)
{
	int ret;
	int n;
	sqlite3 *conn = NULL;
	sqlite3_stmt *statement = NULL;
	char query[MAX_QUERY_LENGTH] = {0};

	n = snprintf(query, MAX_QUERY_LENGTH, "INSERT INTO users VALUES ('%d', '%s', '%s')", user.money, user.email, user.hash);
	if (n < 0 || MAX_QUERY_LENGTH <= n)
	{
		return DB_QUERY_ERROR;
	}

	// アクセス
	ret = sqlite3_open("db_test.sqlite3", &conn);
	if (ret != SQLITE_OK)
	{
		return DB_OPEN_ERROR;
	}

	ret = sqlite3_prepare_v2(conn, query, -1, &statement, NULL);
	if (ret != SQLITE_OK)
	{
		return DB_PROCESS_ERROR;
	}
	sqlite3_step(statement);

	// クローズ
	sqlite3_finalize(statement);
	ret = sqlite3_close(conn);
	if (ret != SQLITE_OK)
	{
		return DB_CLOSE_ERROR;
	}
	return DB_NO_ERROR;
}