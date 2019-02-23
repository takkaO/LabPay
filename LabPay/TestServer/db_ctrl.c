#include "db_ctrl.h"

#define MAX_QUERY_LENGTH 2048

void GetTimeString(char *s, int s_len){
	time_t timer;
	struct tm *local;

	// 現在時刻を取得
	timer = time(NULL);
	// 日本時間に変換
	local = localtime(&timer);

	snprintf(s, s_len, "%4d/%2d/%2d %2d:%2d", local->tm_year + 1900, local->tm_mon + 1, local->tm_mday, local->tm_hour, local->tm_min);
}

enum DBError InsertBuyHistory(struct UserInfo user, struct ProductInfo product)
{
	int ret;
	int n;
	sqlite3 *conn = NULL;
	sqlite3_stmt *statement = NULL;
	char query[MAX_QUERY_LENGTH] = {0};
	char date[255] = {0};

	GetTimeString(date, sizeof(date));

	n = snprintf(query, MAX_QUERY_LENGTH, "INSERT INTO history VALUES ('%s', '%s', '%d', '%d', '%s')", user.email, product.name, product.amount, product.value * product.amount, date);
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

enum DBError GetProductValue(const char *name, int *value){
	int ret;
	int n;
	sqlite3 *conn = NULL;
	sqlite3_stmt *statement = NULL;
	char query[MAX_QUERY_LENGTH] = {0};

	n = snprintf(query, MAX_QUERY_LENGTH, "SELECT value FROM products WHERE name='%s'", name);
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
		*value = sqlite3_column_int(statement, 0);
	}

	// クローズ
	sqlite3_finalize(statement);
	ret = sqlite3_close(conn);
	if (ret != SQLITE_OK)
	{
		return DB_CLOSE_ERROR;
	}
	return DB_NO_ERROR;
}

enum DBError UpdateUserMoneyValue(struct UserInfo user)
{
	int ret;
	int n;
	sqlite3 *conn = NULL;
	sqlite3_stmt *statement = NULL;
	char query[MAX_QUERY_LENGTH] = {0};
	int money = 0;

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
	return DB_NO_ERROR;
}

enum DBError GetUserEmail(const char *hash, char *email)
{
	int ret;
	int n;
	sqlite3 *conn = NULL;
	sqlite3_stmt *statement = NULL;
	char query[MAX_QUERY_LENGTH] = {0};

	n = snprintf(query, MAX_QUERY_LENGTH, "SELECT email FROM users WHERE password='%s'", hash);
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
		const unsigned char *txt = sqlite3_column_text(statement, 0);
		strcpy(email, txt);
		break;
	}

	// クローズ
	sqlite3_finalize(statement);
	ret = sqlite3_close(conn);
	if (ret != SQLITE_OK)
	{
		return DB_CLOSE_ERROR;
	}
	return DB_NO_ERROR;
}

enum DBError GetUserMoneyValue(const char *hash, int *money)
{
	int ret;
	int n;
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