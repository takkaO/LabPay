@echo off
rem Remove old image
docker rmi xxx
rem Create new image
docker build -t xxx --no-cache=true --force-rm=true . 