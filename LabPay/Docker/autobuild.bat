@echo off
rem Remove old image
docker rmi labpay_server
rem Create new image
docker build -t labpay_server --no-cache=true --force-rm=true --squash . 