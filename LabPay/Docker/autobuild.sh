#!/bin/sh
docker rmi labpay_server
docker build -t labpay_server --no-cache=true --force-rm=true . 