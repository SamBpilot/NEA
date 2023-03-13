//
//  data.swift
//  NEA
//
//  Created by Sam Blewitt on 04/03/2023.
//

import Foundation

///  Creates a global server object using dependancy injection.
class ServerObject : ObservableObject {
    /// Class to load information from the server
    @Published public var server = Load()
}

/// Creates a section of each route
struct routingSect{
    
    /// What train to take
    let train: String
    
    /// How many stops to travel
    let stops: Int
    
    /// What station to get on at
    let getOn: String
    
    /// What station to get off at
    let getOff: String
}

/// Creates a possible station
struct station : Identifiable, Equatable{
    
    /// The ID of the station
    let id: Int
    
    /// The name of the station
    let name: String
    
    /// What lines the station is on
    let lines: [String]
}
