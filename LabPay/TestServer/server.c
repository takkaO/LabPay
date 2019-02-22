#include "server.h"

void ChargeMoney(int sock){
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

	SendCommand(sock, "AMOUNT_MONEY");
	memset(inbuf, 0, sizeof(inbuf));
	if (ReceiveCommand(sock, inbuf, sizeof(inbuf)) == FALSE)
	{
		return;
	}
	user.money = atoi(inbuf);

	if(CheckHashConflict(user.hash) != DB_HASH_CONFLICT){
		SendCommand(sock, "NO_USER");
		return;
	}

	if (UpdateMoneyValue(user) == DB_NO_ERROR){
		SendCommand(sock, "FIN");
	}
	else{
		SendCommand(sock, "ERROR");
	}
}

void RegisterUser(int sock){
	char inbuf[2048];
	struct UserInfo user;
	enum DBError ret;

	SendCommand(sock, "HASH");
	memset(inbuf, 0, sizeof(inbuf));
	memset(user.hash, 0, sizeof(user.hash));
	if(ReceiveCommand(sock, inbuf, sizeof(inbuf)) == FALSE){
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
	if(ret != DB_NO_ERROR){
		if(ret == DB_HASH_CONFLICT){
			SendCommand(sock, "HASH_CONFLICT");
		}
		else{
			SendCommand(sock, "ERROR");
		}
		return;
	}

	if(InsertUserInfo(user) == DB_NO_ERROR){
		SendCommand(sock, "FIN");
	}
	else
	{
		SendCommand(sock, "ERROR");
	}
}

void TestConnection(int sock){
	SendCommand(sock, "FIN");
}

enum CmdCommandNumber ParseCommand(const char *buf)
{
	if(strcmp(buf, "CmdTest") == 0){
		return CmdTest;
	}
	else if (strcmp(buf, "CmdAddUser") == 0)
	{
		return CmdAddUser;
	}
	else if(strcmp(buf, "CmdChargeMoney") == 0){
		return CmdChargeMoney;
	}
	else{
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

void SendCommand(int sock, const char *cmd){
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
