#include "hoge.h"

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

void lntrim(char *str)
{
	char *p;
	p = strchr(str, '\n');
	if (p != NULL)
	{
		*p = '\0';
	}
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

	return (re == 1) ? 1 : 0;
}
