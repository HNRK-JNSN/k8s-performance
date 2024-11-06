#!/bin/bash

WeatherforecastURL=http://localhost:5001/

for i in {1..100}
do 
    echo -n "${i} - "
    curl $WeatherforecastURL/weatherforecast
    curl $WeatherforecastURL/telemetry
    curl $WeatherforecastURL/version
    echo ""
done