// 繰り返し接続可能なTCPサーバ(エラー処理追加)
// 接続相手の情報を表示
#include <stdio.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <fcntl.h>  // for open
#include <unistd.h> // for close
#include <string.h>

int main(){
	int sock0;
	struct sockaddr_in addr;
	struct sockaddr_in client;
	int len;
	int sock;
	int n;
	char buf[2048] = {0};
	char inbuf[2048];

	// ソケットの作成
	sock0 = socket(AF_INET, SOCK_STREAM, 0);
	if (sock0 < 0){
		perror("socket");
		return 1;
	}

	snprintf(buf, 2048, 
	"Receive Test Message");

	addr.sin_family = AF_INET;
	addr.sin_port = htons(65500);
	addr.sin_addr.s_addr = htonl(INADDR_ANY);
	if(bind(sock0, (struct sockaddr *)&addr, sizeof(addr)) != 0){
		perror("bind");
		return 1;
	}

	// 接続待機
	if(listen(sock0, 5) != 0){
		perror("listen");
		return 1;
	}

	while(1){
		printf("Ready\n");
		len = sizeof(client);
		sock = accept(sock0, (struct sockaddr *)&client, &len);
		if (sock < 0){
			perror("accept");
			return 1;
		}
		// クライアントの要求受付
		printf("accept connection from %s, port=%d\n", inet_ntoa(client.sin_addr), ntohs(client.sin_port));

		while(1){
			memset(inbuf, 0, sizeof(inbuf));
			recv(sock, inbuf, sizeof(inbuf), 0);
			// 受信した情報をパースすべき
			printf("%s\n", inbuf);
			if (strcmp(inbuf, "CmdTest\n") == 0){
				write(sock, "FIN\n", (int)strlen("FIN\n"));
			}
			else if (strcmp(inbuf, "CmdAddUser\n") == 0){
				printf("cmd OK");
				write(sock, "HASH\n", (int)strlen("HASH\n"));
				memset(inbuf, 0, sizeof(inbuf));
				recv(sock, inbuf, sizeof(inbuf), 0);
				printf("%s", inbuf);
				write(sock, "UID\n", (int)strlen("UID\n"));
				memset(inbuf, 0, sizeof(inbuf));
				recv(sock, inbuf, sizeof(inbuf), 0);
				printf("%s", inbuf);
				write(sock, "FIN\n", (int)strlen("FIN\n"));
			}
			else{
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