#!/bin/bash

WeatherforecastURL=http://127.0.0.1:60863

for i in {1..100}
do 
    echo -n "${i} - "
    curl $WeatherforecastURL/weatherforecast
    curl $WeatherforecastURL/telemetry
    curl $WeatherforecastURL/version
    echo ""
done