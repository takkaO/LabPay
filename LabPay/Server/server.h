#ifndef SERVER_H
#define SERVER_H

#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <fcntl.h>  // for open
#include <unistd.h> // for close
#include <stdlib.h>
#include "db_ctrl.h"

#define COMMAND_MAX_LENGTH 2048
#define MESSAGE_MAX_LENGTH 1024

enum CmdCommandNumber
{
	CmdTest,	   // 接続テストコマンド番号
	CmdAddUser,	// ユーザ追加コマンド番号
	CmdAddProduct, // 商品追加コマンド番号
	CmdBuyProduct, // 購入コマンド番号
	CmdSendMail,   // メール送信コマンド番号
	CmdRequestHash,
	CmdClientFIN,
	CmdChargeMoney,
	CmdUnknown
};

void BuyProducts(int sock);
void ChargeMoney(int sock);
int ReceiveCommand(int sock, char *buf, unsigned long int buf_size);
void RegisterUser(int sock);
void TestConnection(int sock);
int SendEmail(const char *mail_addr, const char *subject, const char *message);
enum CmdCommandNumber ParseCommand(const char *buf);
void SendCommand(int sock, const char *cmd);
int PrepareServer(int port);
int lntrim(char *str);
int CheckReceivable(int fd);

#endif