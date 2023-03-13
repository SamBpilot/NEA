
import re
import logging

from collections import defaultdict
import heapq as heap
from queue import LifoQueue

from argparse import ArgumentParser
import os

TRANSFER_WEIGHT = 2
STOP_WEIGHT = 1

REGEX_SPLIT_NEXT_STATIONS = re.compile("(?P<station>[^\[]*)(\[(?P<lines>.*)\])*")

LINES_TO_COLOUR = {
  "1": "red",
  "2": "green",
  "3": "blue",
  "4": "orange",
  "5": "purple",
  "6": "yellow",
  "7": "teal",
  "A": "lime",
  "B": "darkgray",
  "C": "cyan",
  "D": "purple",
  "E": "brown",
  "F": "royalblue",
  "G": "magenta",
  "J": "darkslateblue",
  "L": "dodgerblue",
  "M": "mediumorchid",
  "N": "mediumslateblue",
  "Q": "goldenrod",
  "R": "salmon",
  "S": "cornflowerblue",
  "W": "springgreen",
  "Z": "orange",
  "SIR": "khaki",
  "SAME": "white",
  "": "black"
}

import xml.etree.ElementTree as et
import math

GRAPH_HEADER = """
digraph G {{
  fontname="Helvetica,Arial,sans-serif";
  node [fontname="Helvetica,Arial,sans-serif bold" labelloc="c"];
  edge [fontname="Helvetica,Arial,sans-serif" fontsize=8 arrowsize=0.5];
  {0}

  {1}

}}
"""

STATION_TEMPLATE = """
  subgraph cluster_{0} {{
    style=filled;
    color=lightgrey;
    labelloc="b";
    node [style=filled];
    {2}
    label = "{1}";
  }}"""

NODE_ID_TEMPLATE = "{0}_{1}"
NODE_TEMPLATE =\
"""node [label="{0}" fillcolor={2}, color="{3}", penwidth={4}, fontcolor=gray, shape="{5}"]"{1}";"""

EDGE_TEMPLATE = """"{0}" -> "{1}"[{2} color={3} fontcolor={3} penwidth={4} fontsize=8 arrowsize=0.5];"""


class Station:
  def __repr__(self):
    return f"[{self.id}, {self.name}, {self.__lat}, {self.__lng}]"

  def __init__(self, id, name, lines, lat, lng, detailed_surrounding_stations, include_lines):
    self.id = id
    self.name = name
    self.__lat = lat
    self.__lng = lng
    self.__detailed_surrounding_stations = detailed_surrounding_stations
    self.__nodes = [
      Node(self, line)
      for line in lines if line in include_lines or len(include_lines) == 0
    ]

  def node_on_line(self, line):
    ret = list(filter(lambda x: x.line == line, self.__nodes))
    return ret[0] if len(ret) > 0 else None

  def nodes(self):
    for node in self.__nodes:
      yield node


  def weight_to(self, from_lat, from_lng):
    delta_lat = self.__lat - from_lat
    delta_lng = self.__lng - from_lng
    return round((math.sqrt(delta_lat**2 + delta_lng**2)) * 500, 2)


  def build_edges(self, master_station_list, include_changes, include_lines):

    if include_changes is True:
      for node in filter(lambda x: x.line in include_lines or len(include_lines) == 0, self.__nodes):
        nodes_same_station = list(filter(lambda x: x!=node, self.__nodes))
        for node_same_station in nodes_same_station:
          # Other nodes at the same station with a constant weight.
          node.add_edge(node_same_station, TRANSFER_WEIGHT)


    for surrounding_station_id in self.__detailed_surrounding_stations.keys():
      surrounding_station = master_station_list.get(surrounding_station_id, None)
      if surrounding_station is None:
        continue
      for node in self.__nodes:
        if len(include_lines) > 0 and node.line not in include_lines:
          continue
        node_on_same_line = surrounding_station.node_on_line(node.line)

        if node_on_same_line is None:
          continue

        if node_on_same_line.line in self.__detailed_surrounding_stations.get(node_on_same_line.station.id, []):
          weight_to = surrounding_station.weight_to(self.__lat, self.__lng)
          node.add_edge(node_on_same_line, weight_to)


class Edge:

  def __repr__(self):
    return f"Edge: From: [{self.frm}], To: [{self.to}], Weight: {self.weight}"

  def __init__(self, frm, to, weight):
    self.frm = frm
    self.to = to
    self.weight = weight

  def key(self):
    return self.frm.station.id + ":" + self.frm.line + "->" + self.to.station.id + ":" + self.to.line


class Node:

  def __repr__(self):
    return f"<{self.station.name}[{self.station.id}]: {self.line}>"

  def __lt__(self, other):
    return int(self.station.id) < int(other.station.id)

  def __init__(self, station, line):
    self.station = station
    self.line = line
    self.edges = []
    self.key = self.station.id + ":" + str(self.line)

  def add_edge(self, to, weight):
    self.edges.append(Edge(self, to, weight))

class GraphBuilder:

  def __init__(self):
    pass

  def __load_xml_content(self, file_name):
    return et.parse(file_name).getroot()

  def __node_fill_colour(self, node):
    return LINES_TO_COLOUR.get(node.line, LINES_TO_COLOUR[""])

  def __node_line_colour(self, node, start_station, end_station):
    return "white" if node.station == start_station or node.station == end_station else LINES_TO_COLOUR.get(node.line, LINES_TO_COLOUR[""])


  def __edge_colour(self, from_node, to_node):
    if from_node.station == to_node.station:
      return LINES_TO_COLOUR.get("SAME", LINES_TO_COLOUR[""])
    else:

      return LINES_TO_COLOUR.get(from_node.line, LINES_TO_COLOUR[""])

  # {"getOn":"Church Av","getOff":"15 St Prospect Park","stops":2,"train":"F"}
  def route_nodes_to_instructions(self, route_nodes, include_on_off_ids=False):
    ret = []

    if len(route_nodes) > 2:
      while route_nodes[-1].station == route_nodes[-2].station:
        route_nodes.remove(route_nodes[-1])

    previous_node = None
    start_of_current_line_node = None
    current_line_stop_count = 0
    logging.debug("Route Nodes: %s", route_nodes)
    for node in route_nodes:
      logging.debug("Current Node Iteration: Node: %s, Previous Node: %s, Start of Current Line Node: %s, Current Line Stop Count: %s", node, previous_node, start_of_current_line_node, current_line_stop_count)
      if start_of_current_line_node is None:
        start_of_current_line_node = node
      if previous_node is not None:
        if previous_node.line == node.line:
          current_line_stop_count += 1
        else:
          if current_line_stop_count > 0:
            instruction = {
              "getOn": start_of_current_line_node.station.name,
              "getOff": previous_node.station.name,
              "stops": current_line_stop_count,
              "train": previous_node.line
            }
            if include_on_off_ids:
              instruction["getOnID"] = start_of_current_line_node.station.id
              instruction["getOffID"] = previous_node.station.id

            ret.append(instruction)
          start_of_current_line_node = node
          current_line_stop_count = 0

      previous_node = node

    # Final instruction
    instruction = {
      "getOn": start_of_current_line_node.station.name,
      "getOff": node.station.name,
      "stops": current_line_stop_count,
      "train": previous_node.line
    }
    if include_on_off_ids:
      instruction["getOnID"] = start_of_current_line_node.station.id
      instruction["getOffID"] = node.station.id

    ret.append(instruction)

    return ret


  def dijkstra(self, start_station_id, end_station_id, stations_by_id):
    start_station = stations_by_id.get(start_station_id, None)
    if start_station is None:
      logging.error(f"Start station with ID {start_station_id} does not exist!")
      exit(1)
    end_station = stations_by_id.get(end_station_id, None)
    if end_station is None:
      logging.error(f"End station with ID {end_station_id} does not exist!")
      exit(1)

    logging.debug(f"Calculating Route From {start_station} to {end_station}")

    # Everything above this is just getting the start and end station from stations_by_id and complaining if they don't exist.

    # Start of actual Dijkstra Algorithm
    start_node = list(start_station.nodes())[0]
    parents_map = {}
    stack = []
    node_weights = defaultdict(lambda: float('inf')) # Will populate and return max float value if you ask for a key not currently in the dictionary.
    node_weights[start_node.key] = 0
    stack.append(start_node)

    # This is actual algorithm.  Even the few lines above are just setting up data variables.
    # The actual algorithm is TEN LINES of actual code (not counting the blank line.)
    while stack:
      node = stack.pop()

      for edge in node.edges:
        adjacent_node = edge.to
        weight = edge.weight
        new_weight = node_weights[node.key] + weight + STOP_WEIGHT
        if node_weights[adjacent_node.key] > new_weight:
          parents_map[adjacent_node.key] = node
          node_weights[adjacent_node.key] = new_weight
          stack.append(adjacent_node) # Only add next node to the list when it's clearly a better path.  This ensures the algorithm will eventually exhaust itself of nodes to visit.
    # End of actual Dijkstra Algorithm
    # Everything after this is just building the list of nodes, edges and stations so you
    # can use it to render the graph.


    route_nodes = []
    route_edges = []
    current_node = list(end_station.nodes())[0]
    route_nodes.append(current_node)
    previous_node = parents_map.get(current_node.key)
    if previous_node is not None:
      relevant_edges = list(filter(lambda e: e.to == current_node, previous_node.edges))
      route_edges.append(relevant_edges[0])
    while previous_node is not None:
      route_nodes.append(previous_node)
      current_node = previous_node
      previous_node = parents_map.get(current_node.key)
      if previous_node is not None:
        relevant_edges = list(filter(lambda e: e.to == current_node, previous_node.edges))
        route_edges.append(relevant_edges[0])

    route_nodes.reverse()
    route_edges.reverse()
    route_stations = list(set(
      [
        x.station
        for x in route_nodes
      ]
    ))
    logging.debug(f"Route visits {len(route_nodes)-2} intervening nodes")
    return route_stations, route_nodes, route_edges


  def build_graph(self, input_file, include_lines, include_changes):

    root = self.__load_xml_content(input_file)

    stations_by_id = {}

    for row in root.findall(".//row"):
      name = row.find("Stop_Name").text
      id = row.find("Station_ID").text
      lines = row.find("Daytime_Routes").text.split(" ")

      if len(list(filter(lambda x: len(include_lines) == 0 or x in include_lines, lines))) == 0:
        continue
      lat = float(row.find("GTFS_Latitude").text)
      lng = float(row.find("GTFS_Longitude").text)
      surrounding_stations = list(filter(lambda x: x.strip() != "" and x != " ", map(str.strip, row.find("Surrounding_Stations").text.split("|"))))
      detailed_surrounding_stations = {}
      for surrounding_station in surrounding_stations:
        matches = REGEX_SPLIT_NEXT_STATIONS.match(surrounding_station)
        next_station_id = matches["station"]
        next_station_lines = matches["lines"]
        if next_station_lines is None:
          # Default to all lines the station is on.
          next_station_lines = lines
        else:
          next_station_lines = next_station_lines.split(",")

        detailed_surrounding_stations.setdefault(next_station_id, next_station_lines)


      stations_by_id[id] = Station(str(id), name, lines, lat, lng, detailed_surrounding_stations, include_lines)

    for station in stations_by_id.values():
      station.build_edges(stations_by_id, include_changes, include_lines)

    return stations_by_id

  def __edge_pensize(self, route_stations_only, route_edges, edge):
    if route_stations_only:
      return 1
    else:
      if edge in route_edges:
        return 5
      else:
        return 1


  def __node_pensize(self, route_stations_only, route_nodes, node):
    if route_stations_only:
      return 1
    else:
      if node in route_nodes:
        return 3
      else:
        return 1

  def __node_shape(self, node, start_station, end_station):
    return "house" if node.station == start_station else "octagon" if node.station == end_station else "circle"


  def process(self, args):
    if (args.start_station is not None or args.end_station is not None) and (args.start_station is None or args.end_station is None):
      logging.error("Must specify BOTH --start and --end!")
      exit(1)

    route_specified = False

    include_lines = list(filter(lambda x: x.strip() != "", map(str.strip, args.include_lines.split(","))))
    include_changes = bool(args.include_changes)
    stations_by_id = self.build_graph(args.input_file, include_lines, include_changes)
    route_lines_only = False
    if args.start_station:
      route_specified = True
      route_stations, route_nodes, route_edges = self.dijkstra(args.start_station, args.end_station, stations_by_id)
      include_lines = list(set(map(lambda x: x.line, route_nodes)))
      route_lines_only = bool(args.route_lines_only) and route_specified
      if route_lines_only:
        logging.debug("Re-building graph using only the lines the calculated route touches: %s", include_lines)
        stations_by_id = self.build_graph(args.input_file, include_lines, include_changes)
        route_stations, route_nodes, route_edges = self.dijkstra(args.start_station, args.end_station, stations_by_id)
      start_station = stations_by_id[args.start_station]
      end_station = stations_by_id[args.end_station]
      logging.debug(f"{len(route_stations)} stations, {len(route_nodes)} nodes, {len(route_edges)} edges on {len(list(set(map(lambda x: x.line, route_nodes))))} lines")
    else:
      route_stations = []
      route_nodes = []
      route_edges = []
      start_station = None
      end_station = None

    highlight_route = bool(args.highlight_route) and route_specified
    route_stations_only = bool(args.route_stations_only) and route_specified

    show_edge_weights = bool(args.show_edge_weights)

    station_output = []
    edge_output = []
    for station in stations_by_id.values():
      if route_stations_only and station not in route_stations:
        continue
      node_output = []
      for node in station.nodes():
        if route_stations_only and node not in route_nodes:
          continue
        node_shape = self.__node_shape(node, start_station, end_station)
        node_fill_colour = self.__node_fill_colour(node)
        node_line_colour = self.__node_line_colour(node, start_station, end_station)
        node_pensize = self.__node_pensize(route_stations_only, route_nodes, node)
        node_output.append(
          NODE_TEMPLATE.format(
            node.line,
            NODE_ID_TEMPLATE.format(node.station.id, node.line),
            node_fill_colour,
            node_line_colour,
            node_pensize,
            node_shape
          )
        )
        for edge in node.edges:
          if route_stations_only and (edge.to not in route_nodes or edge not in route_edges):
            continue
          edge_pensize = self.__edge_pensize(route_stations_only, route_edges, edge)
          # edge_pensize = 8 if highlight_route and edge in route_edges else 1
          edge_colour = self.__edge_colour(node, edge.to)
          edge_text = EDGE_TEMPLATE.format(
            NODE_ID_TEMPLATE.format(node.station.id, node.line),
            NODE_ID_TEMPLATE.format(edge.to.station.id, edge.to.line),
            f"xlabel={edge.weight}," if show_edge_weights else "",
            edge_colour,
            edge_pensize
          )
          edge_output.append(edge_text)


      node_text = "\n    ".join(node_output)

      station_output.append(STATION_TEMPLATE.format(station.id, station.name + " (" + station.id + ")", node_text))

    graphvis_output = GRAPH_HEADER.format(
      "\n".join(station_output),
      "\n  ".join(edge_output)
    )


    with open(args.output_dot_file, "w") as f:
      f.writelines(graphvis_output)

    print(f"Wrote {args.output_dot_file}")

    generate = args.generate == "True"
    open_output = args.open_output == "True"

    if generate is True:
      command = f"dot -O -T{args.output_format} {args.output_dot_file}"
      os.system(command)

    if generate is True and open_output is True:
      command = f"open {args.output_dot_file}.{args.output_format}"
      os.system(command)

if __name__ =="__main__":

  def __prepare_command_line_args():
    arg_parser = ArgumentParser(add_help=True)
    arg_parser.print_usage = arg_parser.print_help

    mandatory_group = arg_parser.add_argument_group(
      "Mandatory Arguments",
      "You must supply a value for each of these parameters"
    )
    mandatory_group.add_argument("--input", required=True, action="store", dest="input_file", help="Specify path to input stations.xml file.")

    optional_group = arg_parser.add_argument_group(
      "Optional Options",
      "Optional parameters with sensible defaults if omitted"
    )
    optional_group.add_argument("--output", required=False, action="store", dest="output_dot_file", help="Specify the name of the output 'dot' (graphviz) file", default="./output.dot")
    optional_group.add_argument("--format", required=False, action="store", dest="output_format", help="Specify a valid argument to pass to -T for the dot command.", default="pdf")
    optional_group.add_argument("--generate", required=False, action="store_const", dest="generate", const="True", help="Specify if you want the generated 'dot' (graphvis) file to be processed into the output format automatically.")
    optional_group.add_argument("--open", required=False, action="store_const", dest="open_output", const="True", help="Specify if you want the generated output to be opened in the default OS application once generated")
    optional_group.add_argument("--lines", required=False, action="store", dest="include_lines", help="Only include these lines (comma separated list)", default="")

    details_group = arg_parser.add_argument_group(
      "Detail Arguments",
      "Specify values to control the detail of the output."
    )
    details_group.add_argument("--changes", required=False, action="store_const", const="True", dest="include_changes", help="Show intra station links (changes).", default=False)
    details_group.add_argument("--weights", required=False, action="store_const", const="True", dest="show_edge_weights", help="Show edge weights", default=False)
    details_group.add_argument("--highlight-route", required=False, action="store_const", const="True", dest="highlight_route", help="Emphasize the route on any rendered output", default=False)
    details_group.add_argument("--route-lines-only", required=False, action="store_const", const="True", dest="route_lines_only", help="Only render lines in any output that a calculated route actually uses.", default=False)
    details_group.add_argument("--route-stations-only", required=False, action="store_const", const="True", dest="route_stations_only", help="Only render stations in any output that a calculated route actually uses.", default=False)


    routing_group = arg_parser.add_argument_group(
      "Routing Arguments",
      "Specify values to control the calculation of a route using Dijkstra."
    )
    routing_group.add_argument("--start", required=False, action="store", dest="start_station", help="Specify start using the station ID.")
    routing_group.add_argument("--end", required=False, action="store", dest="end_station", help="Specify end using the station ID.")

    return arg_parser

  arg_parser = __prepare_command_line_args()
  args = arg_parser.parse_args()
  builder = GraphBuilder()
  builder.process(args)
