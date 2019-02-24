# ベースとなるイメージを選択
FROM ubuntu

# execute command when docker build
RUN echo "Change apt source server"
RUN sed -i -e 's%http://.*.ubuntu.com%http://ftp.jaist.ac.jp/pub/Linux%g' /etc/apt/sources.list
RUN apt update
RUN apt upgrade -y

# イメージでのcd
WORKDIR /srv

# Dockerfileのディレクトリの中身をイメージの中のWORKDIRにコピー
ADD https://raw.githubusercontent.com/takkaO/LabPay/master/LabPay/Server/Makefile .
ADD https://raw.githubusercontent.com/takkaO/LabPay/master/LabPay/Server/server_main.c .
ADD https://raw.githubusercontent.com/takkaO/LabPay/master/LabPay/Server/server.c .
ADD https://raw.githubusercontent.com/takkaO/LabPay/master/LabPay/Server/server.h .
ADD https://raw.githubusercontent.com/takkaO/LabPay/master/LabPay/Server/db_ctrl.c .
ADD https://raw.githubusercontent.com/takkaO/LabPay/master/LabPay/Server/db_ctrl.h .
ADD https://raw.githubusercontent.com/takkaO/LabPay/master/LabPay/Server/db_test.sqlite3.bak .

#ADD https://raw.githubusercontent.com/takkaO/LabPay/develop/LabPay/Server/Makefile .
#ADD https://raw.githubusercontent.com/takkaO/LabPay/develop/LabPay/Server/server_main.c .
#ADD https://raw.githubusercontent.com/takkaO/LabPay/develop/LabPay/Server/server.c .
#ADD https://raw.githubusercontent.com/takkaO/LabPay/develop/LabPay/Server/server.h .
#ADD https://raw.githubusercontent.com/takkaO/LabPay/develop/LabPay/Server/db_ctrl.c .
#ADD https://raw.githubusercontent.com/takkaO/LabPay/develop/LabPay/Server/db_ctrl.h .
#ADD https://raw.githubusercontent.com/takkaO/LabPay/develop/LabPay/Server/db_test.sqlite3.bak .
RUN cp db_test.sqlite3.bak db_test.sqlite3

EXPOSE 65500

RUN echo "Install gcc"
RUN apt install build-essential -y
RUN echo "Install sqlite"
RUN apt install sqlite libsqlite3-dev -y
RUN echo "Install screen"
RUN apt install screen -y
RUN echo "Build server"
RUN make server
RUN echo "Finish!"