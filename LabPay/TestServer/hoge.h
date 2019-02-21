#ifndef HOGE_H
#define HOGE_H

#include <stdio.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <fcntl.h>  // for open
#include <unistd.h> // for close
#include <string.h>
#include <stdlib.h>

void SendCommand(int sock, const char *cmd);
int PrepareServer(int port);
void lntrim(char *str);
int CheckReceivable(int fd);

#endif