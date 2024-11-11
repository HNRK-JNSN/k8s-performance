#!/bin/bash

WeatherforecastURL=http://127.0.0.1:63754

for i in {1..100}
do 
    echo -n "${i} - "
    curl $WeatherforecastURL/weatherforecast > /dev/null
    curl $WeatherforecastURL/telemetry > /dev/null
    curl $WeatherforecastURL/version > /dev/null
    echo ""
done
