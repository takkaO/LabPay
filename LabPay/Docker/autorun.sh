#!/bin/sh
docker rmi xxx
docker build -t xxx --no-cache=true --force-rm=true . 