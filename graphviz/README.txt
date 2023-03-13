1) Make sure you install graphviz.  If you have brew then it's just: brew install graphviz

2) Run python3 ./graph_stations.py --help to see all the options.

3) Here's some examples

3a)
  Generate for only the SIR, A, L and J lines, include the weights on the edges and generate any inter-station change edges.  Generate it automatically (will fail if the "dot" command which is part of graphviz is not on the path) as a PDF and then open the file.

  python3 ./graph_stations.py --input ../Server/Server/stations.xml --output output/SIR_A_L_J.dot --format pdf --changes --weights --lines=SIR,A,L,J --generate --open

3b)
  Generate (automatically, as a PDF, then open) for all lines, but leave out the changes and drop the weights.

  python3 ./graph_stations.py --input ../Server/Server/stations.xml --output output/ALL_NO_CHANGES_OR_WEIGHTS.dot --format pdf --generate --open


3c)
  Generate (automatically, as a PDF, then open) for all lines, including changes and weights.

  python3 ./graph_stations.py --input ../Server/Server/stations.xml --output output/ALL.dot --changes --weights --format pdf --generate --open
