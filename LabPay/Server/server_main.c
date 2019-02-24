#include "server.h"

enum ServerStatus
{
	Ready,
	CommandParse,
	Processing
};

int main()
{
	int sock0;
	struct sockaddr_in client;
	int len;
	int sock;
	int n;
	enum CmdCommandNumber command;
	enum ServerStatus status;
	char buf[2048] = {0};
	char inbuf[2048];

	sock0 = PrepareServer(65500);
	if (listen(sock0, 5) != 0)
	{
		// 接続待機
		perror("listen");
		return 1;
	}

	status = Ready;
	while (1)
	{
		switch (status)
		{
		case Processing:
			//printf("Processing\n");
			switch (command)
			{
			case CmdTest:
				TestConnection(sock);
				status = CommandParse;
				break;
			case CmdAddUser:
				RegisterUser(sock);
				status = CommandParse;
				break;
			case CmdChargeMoney:
				ChargeMoney(sock);
				status = CommandParse;
				break;
			case CmdBuyProduct:
				BuyProducts(sock);
				status = CommandParse;
				break;
			default:
				SendCommand(sock, "ERROR");
				status = Ready;
				close(sock);
				break;
			}
			break;
		case CommandParse:
			//printf("Command Parse\n");
			if (CheckReceivable(sock) == 0)
			{
				SendCommand(sock, "ERROR");
				status = Ready;
				break;
			}
			memset(inbuf, 0, sizeof(inbuf));
			recv(sock, inbuf, sizeof(inbuf), 0);
			if (lntrim(inbuf) == FALSE)
			{
				SendCommand(sock, "ERROR");
				status = Ready;
				break;
			}
			printf("%s\n", inbuf);
			command = ParseCommand(inbuf);
			status = Processing;
			break;
		case Ready:
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
			status = CommandParse;
			break;
		}
	}
	// サーバ終了
	close(sock0);

	return 0;
}