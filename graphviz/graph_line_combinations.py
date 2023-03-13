# tl;dr lazy

# Simple python script that iterates over all possible combinations of lines
# and puts them into the console.

import os

Lines = ["1","2","3","4","5","6","7","A","B","C","D","E","F","G","J","L","M","N","Q","R","S","W","Z","SIR"]

for i in range(0, len(Lines)):
    os.system(f"python3 ./graph_stations.py --input ../Server/Server/stations.xml --output output/{Lines[i]}.dot --format pdf --changes --lines={Lines[i]} --generate --open")
