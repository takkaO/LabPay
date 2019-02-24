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
COPY ../ .

EXPOSE 65500

RUN echo "Install gcc"
RUN apt install build-essential -y
RUN echo "Install sqlite"
RUN apt install sqlite libsqlite3-dev -y
RUN echo "Install screen"
RUN apt install screen -y
RUN echo "Finish!"