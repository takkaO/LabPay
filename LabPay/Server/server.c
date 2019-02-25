#include "server.h"

#define MAX_PRODUCT_NUM 4

void BuyProducts(int sock)
{
	char inbuf[2048];
	struct UserInfo user;
	enum DBError ret;
	int sync = FALSE;
	int i, k;
	int total_fee = 0;
	struct ProductInfo products[MAX_PRODUCT_NUM];

	SendCommand(sock, "HASH");
	memset(inbuf, 0, sizeof(inbuf));
	memset(user.hash, 0, sizeof(user.hash));
	if (ReceiveCommand(sock, inbuf, sizeof(inbuf)) == FALSE)
	{
		return;
	}
	memcpy(user.hash, inbuf, sizeof(user.hash));

	if (CheckHashConflict(user.hash) != DB_HASH_CONFLICT)
	{
		SendCommand(sock, "NO_USER");
		return;
	}

	for (i = 0; i < MAX_PRODUCT_NUM; i++)
	{
		SendCommand(sock, "BUY_PRODUCT_NAME");
		memset(inbuf, 0, sizeof(inbuf));
		if (ReceiveCommand(sock, inbuf, sizeof(inbuf)) == FALSE)
		{
			SendCommand(sock, "ERROR");
			return;
		}
		if (ParseCommand(inbuf) == CmdClientFIN)
		{
			break;
		}
		memcpy(products[i].name, inbuf, sizeof(products[i].name));

		SendCommand(sock, "BUY_PRODUCT_AMOUNT");
		memset(inbuf, 0, sizeof(inbuf));
		if (ReceiveCommand(sock, inbuf, sizeof(inbuf)) == FALSE)
		{
			SendCommand(sock, "ERROR");
			return;
		}
		sync = TRUE;
		if (ParseCommand(inbuf) == CmdClientFIN)
		{
			break;
		}
		products[i].amount = atoi(inbuf);
		sync = FALSE;
	}

	if(sync == FALSE){
		// まだ残っているか聞く（商品最大数より多ければおかしい）
		memset(inbuf, 0, sizeof(inbuf));
		if (ReceiveCommand(sock, inbuf, sizeof(inbuf)) == FALSE)
		{
			SendCommand(sock, "ERROR");
			return;
		}
		if (ParseCommand(inbuf) != CmdClientFIN)
		{
			SendCommand(sock, "ERROR");
			return;
		}
	}

	char p[MESSAGE_MAX_LENGTH] = {0};
	char q[MESSAGE_MAX_LENGTH] = {0};
	for(k = 0; k<i; k++){
		products[k].value = 0;
		GetProductValue(products[k].name, &products[k].value);
		total_fee = total_fee + products[k].value * products[k].amount;

		memcpy(q, p, sizeof(p));
		snprintf(p, MESSAGE_MAX_LENGTH, "%s%s　%d個　計%d円\n", q, products[k].name, products[k].amount, products[k].value * products[k].amount);
	}

	if(GetUserMoneyValue(user.hash, &user.money) != DB_NO_ERROR){
		SendCommand(sock, "ERROR");
		return;
	}

	if(user.money < total_fee){
		SendCommand(sock, "NO_ENOUGH_MONEY");
		return;
	}

	user.money = user.money - total_fee;
	if(UpdateUserMoneyValue(user) != DB_NO_ERROR){
		SendCommand(sock, "ERROR");
		return;
	}
	SendCommand(sock, "FIN");
	char buf[MESSAGE_MAX_LENGTH] = {0};
		snprintf(buf, MESSAGE_MAX_LENGTH, 
		"'商品の購入が完了しました．\n"
		"購入した商品は以下の通りです．\n\n"
		"%s"
		"\nあなたの残高は，%d円です\n"
		"楽しい Lab Life をお送りください．\n'", p, user.money);
		SendEmail(user.email, "[LabPay]商品の購入が完了しました．", buf);

	GetUserEmail(user.hash, user.email);
	for(k = 0; k<i; k++){
		InsertBuyHistory(user, products[k]);
	}
}

void ChargeMoney(int sock)
{
	char inbuf[2048];
	struct UserInfo user;
	enum DBError ret;
	int money = 0;

	SendCommand(sock, "HASH");
	memset(inbuf, 0, sizeof(inbuf));
	memset(user.hash, 0, sizeof(user.hash));
	if (ReceiveCommand(sock, inbuf, sizeof(inbuf)) == FALSE)
	{
		return;
	}
	memcpy(user.hash, inbuf, sizeof(user.hash));

	SendCommand(sock, "AMOUNT_MONEY");
	memset(inbuf, 0, sizeof(inbuf));
	if (ReceiveCommand(sock, inbuf, sizeof(inbuf)) == FALSE)
	{
		return;
	}
	user.money = atoi(inbuf);

	if (CheckHashConflict(user.hash) != DB_HASH_CONFLICT)
	{
		SendCommand(sock, "NO_USER");
		return;
	}

	if( GetUserMoneyValue(user.hash, &money) != DB_NO_ERROR){
		SendCommand(sock, "NO_USER");
		return;
	}

	user.money += money;
	if (UpdateUserMoneyValue(user) == DB_NO_ERROR)
	{
		SendCommand(sock, "FIN");
		char buf[MESSAGE_MAX_LENGTH] = {0};
		snprintf(buf, MESSAGE_MAX_LENGTH, 
		"'あなたのアカウントへのチャージが完了しました．\n"
		"現在の残高は%d円です．\n"
		"楽しい Lab Life をお送りください．\n'", user.money);
		SendEmail(user.email, "[LabPay]チャージが完了しました．", buf);
	}
	else
	{
		SendCommand(sock, "ERROR");
	}
}

void RegisterUser(int sock)
{
	char inbuf[2048];
	struct UserInfo user;
	enum DBError ret;

	SendCommand(sock, "HASH");
	memset(inbuf, 0, sizeof(inbuf));
	memset(user.hash, 0, sizeof(user.hash));
	if (ReceiveCommand(sock, inbuf, sizeof(inbuf)) == FALSE)
	{
		return;
	}
	memcpy(user.hash, inbuf, sizeof(user.hash));
	SendCommand(sock, "EMAIL");
	memset(inbuf, 0, sizeof(inbuf));
	memset(user.email, 0, sizeof(user.email));
	if (ReceiveCommand(sock, inbuf, sizeof(inbuf)) == FALSE)
	{
		return;
	}
	memcpy(user.email, inbuf, sizeof(user.email));
	//printf("hash:%s\nemail:%s\n", user.hash, user.email);
	user.money = 0;

	ret = CheckHashConflict(user.hash);
	if (ret != DB_NO_ERROR)
	{
		if (ret == DB_HASH_CONFLICT)
		{
			SendCommand(sock, "HASH_CONFLICT");
		}
		else
		{
			SendCommand(sock, "ERROR");
		}
		return;
	}

	if (InsertUserInfo(user) == DB_NO_ERROR)
	{
		SendCommand(sock, "FIN");
		char buf[MESSAGE_MAX_LENGTH] = {0};
		snprintf(buf, MESSAGE_MAX_LENGTH, 
		"'LabPayへの登録が完了しました．\n"
		"楽しい Lab Life をお送りください．\n'");
		SendEmail(user.email, "ようそこLabPayへ", buf);
	}
	else
	{
		SendCommand(sock, "ERROR");
	}
}

void TestConnection(int sock)
{
	SendCommand(sock, "FIN");
}

int SendEmail(const char *mail_addr, const char *subject, const char *message){
	int result;
	char buf[COMMAND_MAX_LENGTH] = {0};
	snprintf(buf, COMMAND_MAX_LENGTH, "python3 send_gmail.py %s %s %s", mail_addr, subject, message);
	result = system(buf);
	//printf("%d %s\n", result, buf);
	if(result == EXIT_SUCCESS){
		return TRUE;
	}
	else{
		return FALSE;
	}
}

enum CmdCommandNumber ParseCommand(const char *buf)
{
	if (strcmp(buf, "CmdTest") == 0)
	{
		return CmdTest;
	}
	else if (strcmp(buf, "CmdAddUser") == 0)
	{
		return CmdAddUser;
	}
	else if (strcmp(buf, "CmdChargeMoney") == 0)
	{
		return CmdChargeMoney;
	}
	else if (strcmp(buf, "CmdBuyProduct") == 0)
	{
		return CmdBuyProduct;
	}
	else if (strcmp(buf, "CmdClientFIN") == 0)
	{
		return CmdClientFIN;
	}
	else
	{
		return CmdUnknown;
	}
}

int ReceiveCommand(int sock, char *buf, unsigned long int buf_size)
{
	char inbuf[2048];
	do
	{
		if (CheckReceivable(sock))
		{
			memset(inbuf, 0, sizeof(inbuf));
			recv(sock, inbuf, sizeof(inbuf), 0);
			snprintf(buf, buf_size, "%s%s", buf, inbuf);
		}
		else
		{
			SendCommand(sock, "ERROR");
			return FALSE;
		}
	} while (lntrim(buf) == FALSE);
	return TRUE;
}

void SendCommand(int sock, const char *cmd)
{
	char s[255] = {0};
	snprintf(s, 255, "%s%s", cmd, "\n");
	write(sock, s, (int)strlen(s));
}

int PrepareServer(int port)
{
	int sock0;
	struct sockaddr_in addr;
	// ソケットの作成
	sock0 = socket(AF_INET, SOCK_STREAM, 0);
	if (sock0 < 0)
	{
		perror("socket");
		return 1;
	}

	addr.sin_family = AF_INET;
	addr.sin_port = htons(port);
	addr.sin_addr.s_addr = htonl(INADDR_ANY);
	if (bind(sock0, (struct sockaddr *)&addr, sizeof(addr)) != 0)
	{
		perror("bind");
		exit(1);
	}

	return sock0;
}

int lntrim(char *str)
{
	char *p;
	p = strchr(str, '\n');
	if (p != NULL)
	{
		*p = '\0';
		return TRUE;
	}
	return FALSE;
}

int CheckReceivable(int fd)
{
	fd_set fdset;
	int re;
	struct timeval timeout;

	FD_ZERO(&fdset);
	FD_SET(fd, &fdset);

	/* timeoutは０秒。つまりselectはすぐ戻ってく る */
	timeout.tv_sec = 10;
	timeout.tv_usec = 0;

	/* readできるかチェック */
	re = select(fd + 1, &fdset, NULL, NULL, &timeout);

	return (re == 1) ? TRUE : FALSE;
}
