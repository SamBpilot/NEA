# my approach for the mapping

I think my best bet at generating nodes is doing it when looping through the XML file
Thus I think that when doing so I can generate a node for each station and generate a new "station" object for each station
Each station is at a node and each station has a list of connections to other stations. Each station object will have a name, a list of connection and a list of lines that pass through it.


# useful stuff
https://stackoverflow.com/questions/42284805/shortest-path-on-graph-with-changing-weights