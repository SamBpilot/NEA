# Custom Clients

Developers are allowed to create their own clients for the server. As such, an ecosystem of clients available for different platforms is available. This allows for a more customised experience for the user. The client app is a dummy app that is used to connect to the NEA server, however can work as a base for a cusotom client. To build a custom client, it is recommended that you have experience in the platform you are developing for.

## Connecting to the server

In order to handle multiple devices at once, the server has a special connection protocol. It works as follows:

1) The client connects to the server on port 55600.
2) The client sends to the server `<bind>`
3) The server responds with `<Port:{}` integer `{}Key:{}` string `{}>`
4) The server automatically closes the connection and reopens on port 55600 and the port listed in the response.
5) The server waits for the client to connect to the new port.
6) When the client has connected to the __new__ port the server expects to recieve `<Key:{}` followed by the key from the response. `>`

> [!NOTE]
> Commands are in short form and ARE case sensitive.


## Command List

Commands are **__ALWAYS__** prefixed and suffixed by `<>` .This is to ensure that the server can parse the command correctly. There is no action required for spaces, such as inbetween station names. The following commands are available to the client:

# [bind](#tab/bind)

Command Description: Bind's the client to the server.

Example Call: `<bind>`

Example Response: `<Port:{}1234{}Key:{}testKey>`

Command Limitations: Can only me used on port 55600

# [EOF](#tab/EOF)

Command Description: Ends the connection to the server.

Example Call: `<EOF>`

Example Response: Void

Command Limitations: None

> [!WARNING]
> Please check your client's language for the correct way to handle closing a connection.

> [!WARNING]
> Please handle connections gracefully where possible. Clients that intentionally do not handle connections gracefully will be deny-listed from the server.

# [Key](#tab/Key)

Command Description: Sends the key to the server.

Example Call: `<Key:{}testKey>`

Example Response: Void, if the key is correct.

Command Limitations: Can only be used on ports that __aren't__ 55600 **and** this is the first command sent.

# [SBW](#tab/SBW)

Command Description: Get's any Stations that begin with a phrase

Example Call: `<SBW:{}test>`

Example Response: `<Stations:{}test1{}test2{}test3>` up to 5 stations

Command Limitations: Can only be used on ports that aren't 55600.

# [IVS](#tab/IVS)

Command Description: Checks if a station is valid.

Example Call: `<IVS:{}test>`

Example Response: `<true>`

Command Limitations: Can only be used on ports that aren't 55600.

# [GR](#tab/GR)

Command Description: Get's the route between two stations.

Example Call: `<GR:{}test1{}test2>`

Example Response: `[{"getOn":"test1","getOff":"test3","stops":5,"train":"A"},{"getOn":"test3","getOff":"test2","stops":5,"train":"2"}]`

Command Limitations: Can only be used on ports that aren't 55600.