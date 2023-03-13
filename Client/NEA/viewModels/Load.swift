//
//  Load.swift
//  NEA
//
//  Created by Sam Blewitt on 10/09/2022.
//

import Foundation
import Network

/// Class to load information from the server
class Load : TCPClient{
    
    /// Initialises the connection to the server and handles the reconnection
    /// - Parameters
    ///   - ip: The IP of the server to connect to
    /// - Returns: True if a succesful connection is established
    /// - Throws: invalidIP if the IP is invalid, failedToContact if the connection could not be established, cancelled if the connection to the server is cancelled or unkownError if the error is not known.
    public func initialiseServer(ip : NWEndpoint.Host) throws -> Bool
    {
        var connected = false
        
        super.ip = ip

        do
        {
            connected = try super.connect(ip: super.ip)
        }
        catch NEA.invalidIP { throw NEA.invalidIP }
        catch NEA.failedToContact { throw NEA.failedToContact }
        catch NEA.cancelled {throw NEA.cancelled}
        catch {throw NEA.unkownError} // catch all
        
        return connected
    }
    
    /// Get's an array of stations that begin with the same name
    /// - Parameter name: The Stations to find with
    /// - Returns: An array of up to 5 stations that could begin with the string.
    /// - Throws: unkownError if the response back is malformed
    public func stationBeginsWith(name: String) throws -> [station]
    {
        var response = ""

        do
        {
            response = try super.sendTCP("<SBW:{}\(name)>") // Stations begining with: Name
        }
        catch {throw NEA.unkownError}
        response = self.decode(input: response).0
        var returnedStations = [station]()

        // example of a potential response where name = Gr
        // [Id: 189, name: Grant Av, Lines: A]{}[Id: 509, name: Grant City, Lines: SIR]{}[Id: 231, name: Grand St, Lines: B,D]{}[Id: 505, name: Grasmere, Lines: SIR]{}[Id: 123, name: Grand St, Lines: L]

        let stationArr: [String] = response.components(separatedBy: "{}")
        var Id: Int = -1
        var Name: String = "An error occured"
        
        
        for stop: String in stationArr
        {
            let keyToMap: [String] = stop.components(separatedBy: ", ")
            for key: String in keyToMap
            {
                let keyArr: [String] = key.components(separatedBy: ": ")
                
                guard keyArr.count == 2 else {continue}
                let key: String = keyArr[0]
                let value: String = keyArr[1]
                                                
                if key == "Id" { Id = Int(String(describing: value))!}
                else if key == "Name" { Name = String(describing: value) }
                else if key == "Lines"
                {
                    var newLines: [String] = [String]()
                    // Lines will look like B,D or Q or SIR or B,D,A,C etc..
                    for line: String in value.components(separatedBy: ",")
                    {
                        newLines.append(line)
                    }
                    
                    returnedStations.append(station(id: Id, name: Name, lines: newLines))
                }
                else
                {
                    throw NEA.unkownError // we have absaloutely no idea what the server sent us
                }
            }
        }
        return returnedStations
    }
    
    /// Get's a route between two stations
    /// - Parameters:
    ///   - start: The starting station's ID
    ///   - end: The ending station's ID
    /// - Returns: An array of sections for the route
    /// - Throws:failedToContact if the connection could not be established, cancelled if the connection to the server is cancelled or unkownError if the error is not known.
    public func getRoute(start: Int, end: Int) throws -> Array<routingSect>
    {
        var response: String = ""
        do
        {
            
            response = try super.sendTCP("<GR:{}\(start){}\(end)>") // Route Me start - Stop
            sleep(2) // sometimes it can take a while for the route to process
            super.receive()
            response = super.last_message
            
            guard response.contains("stops") else {throw NEA.unkownError} // verify that a route was actually sent
            
            guard response.contains("Invalid Station") == false else {throw NEA.invalidStation}
            
            response = decode(input: response).0
        }
        catch NEA.failedToContact { throw NEA.failedToContact }
        catch NEA.cancelled {throw NEA.cancelled}
        catch {throw NEA.unkownError} // catch all
        guard response != "" else {throw NEA.failedToContact}

        var returnedStations = [] as [routingSect]
        
        let stationArr: [String] = response.components(separatedBy: "}")
                
        // this will leave something like this for each stop
        // "getOn":"Richmond Valley","getOff":"Great Kills","stops":5,"train":"SIR"
        // now we just need to transform those into an object
        
        var on: String = ""
        var off: String = ""
        var stops: Int = 0
        var train: String = ""
        
        for station: String in stationArr
        {
            let keyToMap: [String] = station.components(separatedBy: ",")
            
            for key: String in keyToMap
            {
                if key == "" {continue}
                let keyArr: [String] = key.components(separatedBy: ":")
                let value: String = keyArr[0].components(separatedBy: "\"")[1]
                var keyPair: String = ""
                if value == "stops"
                {
                    keyPair = keyArr[1]
                }
                else
                {
                    keyPair = keyArr[1].components(separatedBy: "\"")[1]
                }
                
                if value == "getOn" { on = String(describing: keyPair) }
                else if value == "stops" { stops = Int(String(describing: keyPair))! }
                else if value == "train" { train = String(describing: keyPair) }
                else if value == "getOff" { off = String(describing: keyPair) }
                else
                {
                    throw NEA.cancelled // we got something we don't know
                }
                
                if on != off && off != train && train != "" && stops != 0
                {
                    // if everything is filled
                    returnedStations.append(routingSect(train: train, stops: stops, getOn: on, getOff: off))
                    on = ""
                    off = ""
                    stops = 0
                    train = ""
                }
            }
        }
                
        guard returnedStations.count != 0 else {throw NEA.cancelled}
        
        
        return returnedStations //[routingSect(train: "7", stops: 3, getOn: "Somewhere", getOff: "Somewhere else")]
    }
    
    /// decodes the response from the server
    ///  - Parameters:
    ///    - input: The string recieved from the server
    ///  - Returns: The decoded message
    ///  - Remark: If the response is a boolean, it will be returned as a boolean
    private func decode(input:String) -> (String, Bool)
    {
        var output: String = input
        let chars : Set<Character> = ["<", ">", "[", "]", "\\"] // last three are for when we bring json through here
        output.removeAll(where: { chars.contains($0)})
        
        if (output.contains("True"))
        {
            return ("", true)
        }
        else if (output.contains("False"))
        {
            return ("",false)
        }
        else {return (output, false)}
    }
}
