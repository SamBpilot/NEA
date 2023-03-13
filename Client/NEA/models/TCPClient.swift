//
//  TCPClient.swift
//  NEA
//
//  Created by Sam Blewitt on 28/09/2022.
//

import Foundation
import Network

/// Handles sending and recieivng over a network
class TCPClient{
    
    ///  The IP of the host to try and connect to
    public var ip: NWEndpoint.Host
    /// The port of the host of which to connect to
    public var port: NWEndpoint.Port
    /// The last message that was recieved by the server, defualt is "No message found"
    public var last_message: String = "No message found"
    
    
    
    /// The NWConnection object so that we can create and destroy connections as needbe
    private var connection: NWConnection
    /// The key that the server assigned us for our new port
    private var key: String = ""
    
    
    
    /// Initialises the TCP Client class
    init()
    {
        // we must leave the intialiser with defualt values
        self.ip = NWEndpoint.Host("127.0.0.1") // local host
        self.port = NWEndpoint.Port(55600)
        self.connection = NWConnection(host: self.ip, port: self.port, using: .tcp)
    }
    
    /// Connects to the server with a given IP
    /// - Parameters:
    ///   - ip: The IP of the server to connect to
    ///   - port: What port the client should connect to (defualts to 55600)
    /// - Returns: True if everything went alright
    /// - Throws: Cancelled if the state is cancelled, failedToContact if the server could not be contacted, else unkownError
    public func connect(ip: NWEndpoint.Host, port: NWEndpoint.Port = 55600) throws -> Bool
    {
        self.ip = ip
        self.port = port
        self.connection = NWConnection(host: self.ip,port: self.port, using: .tcp)
        var abort: Bool = false
        
        // set's the statehandler
        self.connection.stateUpdateHandler = { (newState: NWConnection.State) in
            
            switch (newState) {
                // if the connection is ready then we can start to recieve data
            case .ready:
                print("State: Ready \n")
                self.receive()
            case .setup:
                print("State Setup \n")
            case .cancelled:
                abort = true
                print("State Cancelled \n")
            case .preparing:
                print("State Preparing \n")
            default:
                print("Connection idling as we are in defualt state.")
            }
        }
        
        // start a connection to the server
        
        self.connection.start(queue: .global())
        
        if (abort == true)
        {
            return false
        }
        
        do
        {
            // if the port is 55600 (aka the defualt port) then try and ask the server to bind to a different port
            if self.port == NWEndpoint.Port(55600){
                try self.reconnect()
            }
        }
        catch NEA.cancelled
        {
            throw NEA.cancelled
        }
        catch NEA.failedToContact
        {
            throw NEA.failedToContact
        }
        catch
        {
            throw NEA.unkownError // catch all case
        }
        
        return true
    }
    
    /// Sends a message to the TCP server
    ///
    /// - Parameter content : The message to send to the TCP server
    /// - Returns : The reply recieved if no error
    /// - Throws: unkownError if an error occured
    public func sendTCP(_ content:String) throws -> String {
        var responseType: Int = 200
        let contentToSendTCP: Data? = content.data(using: String.Encoding.utf8)
        self.connection.send(content: contentToSendTCP, completion: NWConnection.SendCompletion.contentProcessed (({ (NWError: NWError?) in
            if (NWError == nil){
                // all is good
            } else{
                responseType = 418
            }
        })))
        
        if responseType != 200
        {
            throw NEA.unkownError // we don't know what happened or why, just that an error happened. So report it
        }
        
        self.receive()
        return self.last_message
    }
    
    /// Recieves a message via the TCP protocol
    ///
    /// - Returns - The message the server got
    public func receive(){
        self.connection.receive(minimumIncompleteLength: 1, maximumLength: 8192) { (content: Data?, context: NWConnection.ContentContext?, isComplete: Bool, error: NWError?) in
            let response: String? = String(bytes: (content) ?? Data(), encoding: .utf8)
            if content?.count != nil
            {
                self.last_message = response ?? "error"
            }
            
            // this is liekly to crash if it recieves too much data
            // change to a foreach loop and add to the write up a screenshot of the current code as well as an explenation as to why it doesn't scale
            
            if self.connection.state == .ready && isComplete == false {
                self.receive()
            }
        }
    }
    
    /// Gracefully closes the connection between the TCP Server
    ///
    /// - Returns - Void
    public func closeTCP(){
        let content: String = "<EOF>" // end of file
        let contentToSendTCP: Data? = content.data(using: String.Encoding.utf8)
        self.connection.send(content: contentToSendTCP, completion: NWConnection.SendCompletion.contentProcessed(({ (NWError: NWError?) in
            if (NWError == nil){
                print("Succesfully closed the connection")
            } else {
                print("ERROR! Can not close connection to the server \n \(NWError!)")
            }
    })))
    }
    
    /// Reconnects to the server if the port is 55600 (the defualt) at a different port
    /// - Throws: failedToContact if a connection to the server could not be established or unkownError if the error is not known.
    private func reconnect() throws
    {
        do{
            // send a message to the TCP server
            _ = try self.sendTCP("<bind>")
            self.receive()
            sleep(1)
            let portAndKey: [String] = self.last_message.components(separatedBy: "{}")
            guard portAndKey.count > 2 else {throw NEA.failedToContact}
            
            self.port = NWEndpoint.Port(String(portAndKey[1]))!
            self.key = String(portAndKey[3])
            
            // attempt to reconnect to the server with the new port and send back the new key
            
            self.connection = NWConnection(host: self.ip,port: self.port, using: .tcp)
            self.connection.start(queue: .global())
            sleep(1)
            
            _ = try self.sendTCP("<Key:{}" + self.key) // none at the end here because we don't split by > and so it's left over
        }
        catch
        {
            throw NEA.unkownError
        }
    }
}
