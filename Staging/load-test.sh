#!/bin/bash

for i in {1..200}
do 
    echo $i
    curl -s http://localhost:5057/weatherforecast
    curl -s http://localhost:5057/telemetry
    curl -s http://localhost:5057
done