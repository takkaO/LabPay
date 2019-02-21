#include "hoge.h"

int main()
{
	int sock0;
	struct sockaddr_in client;
	int len;
	int sock;
	int n;
	char buf[2048] = {0};
	char command[255];
	char inbuf[2048];

	sock0 = PrepareServer(65500);
	if (listen(sock0, 5) != 0){
		// 接続待機
		perror("listen");
		return 1;
	}

	while (1)
	{
		printf("Ready\n");
		len = sizeof(client);
		sock = accept(sock0, (struct sockaddr *)&client, &len);
		if (sock < 0)
		{
			perror("accept");
			return 1;
		}

		// クライアントの要求受付
		printf("accept connection from %s, port=%d\n", inet_ntoa(client.sin_addr), ntohs(client.sin_port));

		while (1)
		{
			memset(inbuf, 0, sizeof(inbuf));

			if (CheckReceivable(sock) == 0)
			{
				printf("timeout0\n");
				SendCommand(sock, "ERROR");
				break;
			}
			recv(sock, inbuf, sizeof(inbuf), 0);
			lntrim(inbuf);
			printf("%s\n", inbuf);

			if (strcmp(inbuf, "CmdTest") == 0)
			{
				SendCommand(sock, "FIN");
			}
			else if (strcmp(inbuf, "CmdAddUser") == 0)
			{
				SendCommand(sock, "HASH");

				if (CheckReceivable(sock) == 0)
				{
					printf("timeout\n");
					SendCommand(sock, "ERROR");
					break;
				}
				recv(sock, inbuf, sizeof(inbuf), 0);

				SendCommand(sock, "EMAIL");

				if (CheckReceivable(sock) == 0)
				{
					printf("timeout\n");
					SendCommand(sock, "ERROR");
					break;
				}
				recv(sock, inbuf, sizeof(inbuf), 0);

				SendCommand(sock, "FIN");
				close(sock);
			}
			else
			{
				write(sock, "ERROR\n", (int)strlen("ERROR\n"));
				close(sock);
				break;
			}
		}

		//send(sock, buf, (int)strlen(buf), 0);

		// TCPセッションの終了
	}

	// サーバ終了
	close(sock0);

	return 0;
}